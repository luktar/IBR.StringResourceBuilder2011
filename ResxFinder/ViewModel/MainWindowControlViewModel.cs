using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio;
using NLog;
using ResxFinder.Core;
using ResxFinder.Interfaces;
using ResxFinder.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.ViewModel
{
    public class MainWindowControlViewModel : ViewModelBase
    {

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private string text;

        private List<Parser> Parsers { get; set; }
        public RelayCommand PropertiesCommand { get; private set; }
        public RelayCommand RunCommand { get; private set; }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        public MainWindowControlViewModel()
        {
            PropertiesCommand = new RelayCommand(PropertiesPressed);
            RunCommand = new RelayCommand(RunPressed);

            Text = "Press 'Run' button...";
        }

        private void RunPressed()
        {
            Parsers = new List<Parser>();

            ISolutionProjectsHelper solutionProjectHelper = 
                ViewModelLocator.Instance.GetInstance<ISolutionProjectsHelper>();

            foreach (Project project in solutionProjectHelper.GetProjects())
            {

                AnalyzeProjectItems(project.ProjectItems);

            }

            WriteReport();
        }

        private void PropertiesPressed()
        {
            SettingsWindow window = new SettingsWindow(SettingsHelper.Instance.Settings);
            if (window.ShowDialog() ?? false)
            {
                SettingsHelper.Instance.Save();
            }
        }

        private void WriteReport()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Parser parser in Parsers)
            {
                if (parser.StringResources.Count == 0) continue;

                sb.AppendLine("###############################################################################################");
                sb.AppendLine(parser.FileName);
                sb.AppendLine();

                foreach (StringResource stringResource in parser.StringResources)
                {
                    sb.AppendLine($"Name: {stringResource.Name}, text: {stringResource.Text}, location: {stringResource.Location}");
                }

                sb.AppendLine();
            }

            Text = sb.ToString();
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
                csFilePath = projectItem.Properties.Item("FullPath").Value.ToString();
                if (csFilePath.EndsWith(".cs"))
                {

                    if (!projectItem.IsOpen)
                        projectItem.Open();

                    Document document = projectItem.Document;
                    TextDocument textDocument = document.Object("TextDocument") as TextDocument;

                    if (textDocument == null) return;

                    Parser parser =
                        new Parser(projectItem, SettingsHelper.Instance.Settings, StartMenuItemPackage.ApplicationObject);
                    bool result = parser.Start(textDocument.StartPoint, textDocument.EndPoint, textDocument.EndPoint.Line);
                    document.Close();

                    if (!result) return;

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
