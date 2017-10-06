using EnvDTE;
using Microsoft.VisualStudio;
using NLog;
using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class ParserManager : IParserManager
    {
        private const string FULL_PATH = "FullPath";
        private const string CS_EXTESION = ".cs";
        private const string TEXT_DOCUMENT = "TextDocument";

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public List<Parser> Parsers { get; private set; } = new List<Parser>();

        public List<Parser> GetParsers(List<Project> projects)
        {
            Parsers.Clear();
            projects.ForEach(x => AnalyzeProjectItems(x.ProjectItems));
            return Parsers;
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
                csFilePath = projectItem.Properties.Item(FULL_PATH).Value.ToString();
                if (csFilePath.EndsWith(CS_EXTESION))
                {

                    if (!projectItem.IsOpen)
                        projectItem.Open();

                    Document document = projectItem.Document;
                    TextDocument textDocument = document.Object(TEXT_DOCUMENT) as TextDocument;

                    if (textDocument == null) return;

                    Parser parser =
                        new Parser(projectItem, SettingsHelper.Instance.Settings, StartMenuItemPackage.ApplicationObject);
                    bool result = parser.Start(textDocument.StartPoint, textDocument.EndPoint, textDocument.EndPoint.Line);
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
