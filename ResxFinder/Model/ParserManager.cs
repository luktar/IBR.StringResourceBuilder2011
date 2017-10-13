using EnvDTE;
using Microsoft.VisualStudio;
using NLog;
using ResxFinder.Interfaces;
using ResxFinder.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class ParserManager : IParserManager
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public List<IParser> Parsers { get; private set; } = new List<IParser>();

        public List<IParser> GetParsers(List<Project> projects)
        {
            string currentProjectName = string.Empty;
            try
            {
                Parsers.Clear();

                foreach(Project project in projects)
                {
                    currentProjectName = project.Name;
                    try
                    {
                        AnalyzeProjectItems(project.ProjectItems);
                    } catch (Exception e)
                    {
                        logger.Warn(e, $"An error occurred while analyzing project: " + currentProjectName);
                    }
                }

                return Parsers;
            } catch (Exception e)
            {
                string logMessage = "An error occurred while parsing project: " + currentProjectName;
                logger.Warn(logMessage);
                throw new Exception(logMessage, e);
            }
        }

        private void AnalyzeProjectItems(ProjectItems projectItems)
        {
            if (projectItems == null) return;

            foreach (ProjectItem projectItem in projectItems)
            {
                if (projectItem.Kind.Equals(VSConstants.ItemTypeGuid.PhysicalFolder_string))
                {
                    AnalyzeProjectItems(projectItem.ProjectItems);
                    continue;
                }

                AnalyzeFile(projectItem);
            }
        }

        private void AnalyzeFile(ProjectItem projectItem)
        {
            string csFilePath = String.Empty;
            try
            {
                csFilePath = projectItem.Properties.Item(Constants.FULL_PATH).Value.ToString();
                if (csFilePath.EndsWith(Constants.CS_EXTESION))
                {
                    bool wasOpen = projectItem.IsOpen;
                    if (!projectItem.IsOpen) { 
                        projectItem.Open();}

                    Document document = projectItem.Document;
                    TextDocument textDocument = document.Object(Constants.TEXT_DOCUMENT) as TextDocument;

                    if (textDocument == null) return;

                    ISettingsHelper settings = ViewModelLocator.Instance.GetInstance<ISettingsHelper>();

                    FileParser parser =
                        new FileParser(projectItem, settings.Settings);
                    bool result = parser.Start();

                    if(!wasOpen)
                        document.Close();

                    if (!result) return;

                    if (parser.StringResources.Count == 0) return;

                    Parsers.Add(parser);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Unknown problem occurred while analyzing file: " + csFilePath);
            }
        }

    }
}
