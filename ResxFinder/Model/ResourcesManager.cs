﻿using EnvDTE;
using EnvDTE80;
using NLog;
using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace ResxFinder.Model
{
    public class ResourcesManager : IResourcesManager
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private DTE2 Dte { get; set; } = StartMenuItemPackage.ApplicationObject;

        private Settings Settings { get; set; }

        private EnvDTE.Window Window { get; set; }

        private TextDocument TextDocument { get; set; }

        private string ProjectExtension { get; set; }

        private ProjectItem ResourcesFile { get; set; }

        private string ProjectItemFileName { get; set; }

        public ResourcesManager(Settings settings, 
            ProjectItem projectItem, IDocumentsManager documentManager) //ProjectItem projectItem)
        {
            TextDocument = documentManager.GetTextDocument(projectItem);
            ProjectItemFileName = projectItem.Properties.Item(Constants.FULL_PATH).Value.ToString();
            Window = documentManager.OpenWindow(ProjectItemFileName);
            Settings = settings;
            ProjectExtension = System.IO.Path.GetExtension(Window.Project.FullName).Substring(1);
        }

        private  ProjectItem GetResourceFile()
        {
            ProjectItem resourceFilePrjItem = null;

            string resourceFileName = null,
                   resourceFileDir = null;

            if (!Settings.IsUseGlobalResourceFile)
            {
                resourceFileName = System.IO.Path.ChangeExtension(Window.ProjectItem.Name, "Resources.resx"); //file name only
                resourceFileDir = System.IO.Path.GetDirectoryName(Window.ProjectItem.FileNames[0]);
            }
            else
            {
                resourceFileName = ProjectExtension.StartsWith("cs", StringComparison.OrdinalIgnoreCase) ? "Properties" : "My Project";
                resourceFileDir = System.IO.Path.GetDirectoryName(Window.ProjectItem.ContainingProject.FullName);
            }

            //get the projects project-items collection
            ProjectItems prjItems = Window.Project.ProjectItems;

            if (!Settings.IsUseGlobalResourceFile)
            {
                try
                {
                    //try to get the parent project-items collection (if in a sub folder)
                    prjItems = ((ProjectItem)Window.ProjectItem.Collection.Parent).ProjectItems;
                }
                catch { }
            }

            try
            {
                resourceFilePrjItem = prjItems?.Item(resourceFileName);
            }
            catch { }

            if (Settings.IsUseGlobalResourceFile)
            {
                bool isPropertiesItem = (resourceFilePrjItem != null);

                if (isPropertiesItem)
                {
                    prjItems = resourceFilePrjItem.ProjectItems;
                    resourceFilePrjItem = null;
                    resourceFileDir = System.IO.Path.Combine(resourceFileDir, resourceFileName); //append "Properties"/"My Project" because it exists
                }

                if (prjItems == null)
                    return (null); //something went terribly wrong that never should have been possible

                if (string.IsNullOrEmpty(Settings.GlobalResourceFileName))
                    resourceFileName = "Resources.resx"; //standard global resource file
                else
                    resourceFileName = System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(Settings.GlobalResourceFileName), "Resources.resx");

                try
                {
                    //searches for the global resource
                    resourceFilePrjItem = prjItems.Item(resourceFileName);
                }
                catch { }
            }

            if (resourceFilePrjItem == null)
            {
                #region not in project but file exists? -> ask user if delete
                string projectPath = System.IO.Path.GetDirectoryName(Window.ProjectItem.ContainingProject.FullName),
                       resourceFile = System.IO.Path.Combine(resourceFileDir, resourceFileName),
                       designerFile = System.IO.Path.ChangeExtension(resourceFile, ".Designer." + ProjectExtension.Substring(0, 2)/*((m_IsCSharp) ? "cs" : "vb")*/);

                if (System.IO.File.Exists(resourceFile) || System.IO.File.Exists(designerFile))
                {
                    string msg = string.Format("The resource file already exists though it is not included in the project:\r\n\r\n"
                                               + "'{0}'\r\n\r\n"
                                               + "Do you want to overwrite the existing resource file?",
                                               resourceFile.Substring(projectPath.Length).TrimStart('\\'));

                    if (MessageBox.Show(msg, "Make resource", MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                        return (null);
                    else
                    {
                        TryToSilentlyDeleteIfExistsEvenIfReadOnly(resourceFile);
                        TryToSilentlyDeleteIfExistsEvenIfReadOnly(designerFile);
                    }
                }
                #endregion

                try
                {
                    // Retrieve the path to the resource template.
                    string itemPath = ((Solution2)Dte.Solution).GetProjectItemTemplate("Resource.zip", ProjectExtension);

                    //create a new project item based on the template
                    /*prjItem =*/
                    prjItems.AddFromTemplate(itemPath, resourceFileName); //returns always null ...
                    resourceFilePrjItem = prjItems.Item(resourceFileName);
                }
                catch (Exception ex)
                {
                    
                    return null;
                }
            }

            if (resourceFilePrjItem == null)
                return (null);

            //open the ResX file
            if (!resourceFilePrjItem.IsOpen[EnvDTE.Constants.vsViewKindAny])
                resourceFilePrjItem.Open(EnvDTE.Constants.vsViewKindDesigner);

            return (resourceFilePrjItem);
        }

        private void SelectStringInTextDocument(StringResource stringResource)
        {
            try
            {
                if (stringResource == null)
                    return;

                string text = stringResource.Text;
                System.Drawing.Point location = stringResource.Location;
                //bool isAtString = false;

                TextDocument.Selection.MoveToLineAndOffset(location.X, location.Y, false);

                if (location.Y > 1)
                {
                    TextDocument.Selection.MoveToLineAndOffset(location.X, location.Y - 1, false);
                    TextDocument.Selection.CharRight(true, 1);
                    if (TextDocument.Selection.Text[0] != '@')
                        TextDocument.Selection.MoveToLineAndOffset(location.X, location.Y, false);

                    //  isAtString = true;
                }
                TextDocument.Selection.MoveToLineAndOffset(location.X, location.Y + text.Length + 2, true);

                if ((Window != null) && (Window != Dte.ActiveWindow))
                    Window.Activate();
            } catch(Exception e)
            {
                logger.Warn($"Problem with selecting string resource: {stringResource} in file: {ProjectItemFileName}");
                throw e;
            }
        }

        public void WriteToResource(StringResource stringResource)
        {
            try
            {
                SelectStringInTextDocument(stringResource);
                ResourcesFile = GetResourceFile();
                string resxFile = ResourcesFile.Document.FullName;

                string nameSpace = Window.Project.Properties.Item("RootNamespace").Value.ToString();

                #region get namespace from ResX designer file
                foreach (ProjectItem item in ResourcesFile.ProjectItems)
                {
                    if (item.Name.EndsWith(".resx", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    foreach (CodeElement element in item.FileCodeModel.CodeElements)
                    {
                        if (element.Kind == vsCMElement.vsCMElementNamespace)
                        {
                            nameSpace = element.FullName;
                            break;
                        }
                    }
                }
                #endregion

                //close the ResX file to modify it (force a checkout)
                ResourcesFile.Document.Save(); //[13-07-10 DR]: MS has changed behavior of Close(vsSaveChanges.vsSaveChangesYes) in VS2012
                ResourcesFile.Document.Close(vsSaveChanges.vsSaveChangesYes);

                string name,
                       value,
                       comment = string.Empty;

                name = stringResource.Name;
                value = stringResource.Text;

                if (TextDocument.Selection.Text.Length > 0 && TextDocument.Selection.Text[0] == '@')
                    value = value.Replace("\"\"", "\\\""); // [""] -> [\"] (change VB writing to CSharp)

                //add to the resource file (checking for duplicate)
                if (!AppendStringResource(resxFile, name, value, comment))
                    return;

                //(re-)create the designer class
                VSLangProj.VSProjectItem vsPrjItem = ResourcesFile.Object as VSLangProj.VSProjectItem;
                if (vsPrjItem != null)
                    vsPrjItem.RunCustomTool();

                //get the length of the selected string literal and replace by resource call
                int replaceLength = TextDocument.Selection.Text.Length;
                if (replaceLength > 0)
                {
                    string className = System.IO.Path.GetFileNameWithoutExtension(resxFile).Replace('.', '_'),
                           aliasName = className.Substring(0, className.Length - 6), //..._Resources -> ..._Res
                           resourceCall = string.Concat(aliasName, ".", name);

                    bool isGlobalResourceFile = Settings.IsUseGlobalResourceFile && string.IsNullOrEmpty(Settings.GlobalResourceFileName),
                         isDontUseResourceUsingAlias = Settings.IsUseGlobalResourceFile && Settings.IsDontUseResourceAlias;

                    if (isGlobalResourceFile)
                    {
                        //standard global resource file
                        aliasName = string.Concat("Glbl", aliasName);
                        resourceCall = string.Concat("Glbl", resourceCall);
                    }

                    int oldRow = TextDocument.Selection.ActivePoint.Line;

                    if (!isDontUseResourceUsingAlias)
                    {
                        //insert the resource using alias (if not yet)
                        CheckAndAddAlias(nameSpace, className, aliasName);
                    }
                    else
                    {
                        //create a resource call like "Properties.SRB_Strings_Resources.myResText", "Properties.Resources.myResText", "Resources.myResText"
                        int lastDotPos = nameSpace.LastIndexOf('.');
                        string resxNameSpace = (lastDotPos >= 0) ? string.Concat(nameSpace.Substring(lastDotPos + 1), ".") : string.Empty;
                        resourceCall = string.Concat(resxNameSpace, className, ".", name);
                    }

                    //insert the resource call, replacing the selected string literal
                    TextDocument.Selection.Insert(resourceCall, (int)vsInsertFlags.vsInsertFlagsContainNewText);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Unable write string to resource file. File: {ProjectItemFileName}, resource: {stringResource}.");
                throw e;
            }
        }

        private void CheckAndAddAlias(string nameSpace,
                              string className,
                              string aliasName)
        {
            string resourceAlias1 = $"using {aliasName} = ",
                   resourceAlias2 = $"global::{nameSpace}.{className}";


            resourceAlias1 = $"Imports {aliasName} = ";
            resourceAlias2 = $"{nameSpace}.{className}";


            CodeElements elements = TextDocument.Parent.ProjectItem.FileCodeModel.CodeElements;
            CodeElement lastElement = null;
            bool isImport = false;

            #region find alias or last using/Import element (return if alias found)
            foreach (CodeElement element in elements) //not really fast but more safe
            {
                if (!isImport)
                {
                    //find first using/import statement
                    if (element.Kind == vsCMElement.vsCMElementImportStmt)
                        isImport = true;
                }
                else
                {
                    //using/import statement was available so find next NON using/import statement
                    if (element.Kind != vsCMElement.vsCMElementImportStmt)
                        break;
                }

                if (element.Kind == vsCMElement.vsCMElementOptionStmt)
                    //save last option statement
                    lastElement = element;
                else if (element.Kind == vsCMElement.vsCMElementImportStmt)
                {
                    //save last using/import statement
                    lastElement = element;

                    //check if resource alias is already there
                    CodeImport importElement = element as CodeImport;
                    if ((importElement.Alias != null) && importElement.Alias.Equals(aliasName) && importElement.Namespace.Equals(resourceAlias2))
                        return;
                }
            }
            #endregion

            EditPoint insertPoint = null;

            if (lastElement == null)
                insertPoint = TextDocument.CreateEditPoint(TextDocument.StartPoint); //beginning of text
            else
            {
                //behind last element
                insertPoint = lastElement.EndPoint.CreateEditPoint();
                insertPoint.LineDown(1);
                insertPoint.StartOfLine();

                if (lastElement.Kind == vsCMElement.vsCMElementOptionStmt)
                    insertPoint.Insert(Environment.NewLine);
            }

            resourceAlias2 += ";";

            string alias = resourceAlias1 + resourceAlias2 + Environment.NewLine;

            insertPoint.Insert(alias);
        }

        private bool AppendStringResource(string resxFileName,
                                  string name,
                                  string value,
                                  string comment)
        {
            try
            {
                XmlElement dataElement,
                           valueElement;
                XmlAttribute attribute;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(resxFileName);

                string xmlPath = $"descendant::data[(attribute::name='{name}')]/descendant::value";
                XmlNode xmlNode = xmlDoc.DocumentElement.SelectSingleNode(xmlPath);
                if (xmlNode != null)
                {
                    //xmlNode = xmlNode.SelectSingleNode("descendant::value");
                    string msg = string.Format("This resource name already exists:\r\n\r\n"
                                               + "{0} = '{1}'\r\n"
                                               + "(new string is '{2}')\r\n\r\n"
                                               + "Do you want to use the existing resource instead?",
                                               name, xmlNode.InnerText, value);
                    return !(MessageBox.Show(msg, "Make resource", MessageBoxButton.YesNo,
                                        MessageBoxImage.Question) != MessageBoxResult.Yes);
                }

                if (value.Contains("\\r"))
                    value = value.Replace("\\r", "\r");

                if (value.Contains("\\n"))
                    value = value.Replace("\\n", "\n");

                if (value.Contains("\\t"))
                    value = value.Replace("\\t", "\t");

                if (value.Contains("\\0"))
                    value = value.Replace("\\0", "\0");

                if (value.Contains(@"\\"))
                    value = value.Replace(@"\\", @"\");


                if (value.Contains("\\\""))
                    value = value.Replace("\\\"", "\"");

                dataElement = xmlDoc.CreateElement("data");
                {
                    attribute = xmlDoc.CreateAttribute("name");
                    attribute.Value = name;
                    dataElement.Attributes.Append(attribute);

                    attribute = xmlDoc.CreateAttribute("xml:space");
                    attribute.Value = "preserve";
                    dataElement.Attributes.Append(attribute);

                    valueElement = xmlDoc.CreateElement("value");
                    valueElement.InnerText = value;
                    dataElement.AppendChild(valueElement);

                    if (!string.IsNullOrEmpty(comment))
                    {
                        valueElement = xmlDoc.CreateElement("comment");
                        valueElement.InnerText = comment;
                        dataElement.AppendChild(valueElement);
                    }
                }
                xmlDoc.DocumentElement.AppendChild(dataElement);

                xmlDoc.Save(resxFileName);
            }
            catch (Exception ex)
            {
                return (false);
            }

            return (true);
        }

        private static void TryToSilentlyDeleteIfExistsEvenIfReadOnly(string file)
        {
            if (System.IO.File.Exists(file))
            {
                System.IO.FileAttributes attribs = System.IO.File.GetAttributes(file);

                if ((attribs & System.IO.FileAttributes.ReadOnly) != 0)
                    System.IO.File.SetAttributes(file, attribs & ~System.IO.FileAttributes.ReadOnly);

                try { System.IO.File.Delete(file); }
                catch { }
            }
        }
    }
}
