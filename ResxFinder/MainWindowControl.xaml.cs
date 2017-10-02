namespace ResxFinder
{
    using EnvDTE;
    using EnvDTE80;
    using Microsoft.VisualStudio;
    using NLog;
    using ResxFinder.Core;
    using ResxFinder.Views;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for MainWindowControl.
    /// </summary>
    public partial class MainWindowControl : UserControl
    {
        /// <summary>
        /// http://www.wwwlicious.com/2011/03/29/envdte-getting-all-projects-html/
        /// </summary>
        public const string PROJECT_KIND_SOLUTION_FOLDER = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private List<Parser> Parsers { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowControl"/> class.
        /// </summary>
        public MainWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow window = new SettingsWindow(SettingsHelper.Instance.Settings);

            if (window.ShowDialog() ?? false)
            {
                SettingsHelper.Instance.Save();
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Parsers = new List<Parser>();

            foreach(Project project in GetProjects()) { 

                AnalyzeProjectItems(project.ProjectItems);
                
            }

            WriteReport();

        }

        private void AnalyzeSolutionFolder(ProjectItem projectItem)
        {       
            if (projectItem.Kind == PROJECT_KIND_SOLUTION_FOLDER)
            {
                if (projectItem.ProjectItems == null) return;

                foreach (ProjectItem subProjectItem in projectItem.ProjectItems)
                {
                    AnalyzeSolutionFolder(subProjectItem);
                }
                return;
            }

            AnalyzeProjectItems(projectItem.ProjectItems);
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

            textBox.Text = sb.ToString();
        }

        private void AnalyzeProjectItems(ProjectItems projectItems)
        {
            if (projectItems == null) return;

            foreach (ProjectItem projectItem in projectItems)
            {
                if(projectItem.Kind.Equals(VSConstants.ItemTypeGuid.PhysicalFolder_string))
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

        public List<Project> GetProjects()
        {
            Projects projects = StartMenuItemPackage.ApplicationObject.Solution.Projects;
            List<Project> list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                var project = item.Current as Project;
                if (project == null)
                {
                    continue;
                }

                if (project.Kind == PROJECT_KIND_SOLUTION_FOLDER)
                {
                    list.AddRange(GetSolutionFolderProjects(project));
                }
                else
                {
                    list.Add(project);
                }
            }

            return list;
        }

        private List<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == PROJECT_KIND_SOLUTION_FOLDER)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }
    }
}