namespace ResxFinder
{
    using EnvDTE;
    using EnvDTE80;
    using ResxFinder.Core;
    using ResxFinder.Views;
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
            foreach(Project project in StartMenuItemPackage.ApplicationObject.Solution.Projects)
            {
                StringBuilder sb = new StringBuilder();

                foreach(ProjectItem projectItem in project.ProjectItems)
                {
                    string csFilePath = projectItem.Properties.Item("FullPath").Value.ToString();
                    if (csFilePath.EndsWith(".cs"))
                    {
                        sb.AppendLine(csFilePath);

                        if (!projectItem.IsOpen)
                            projectItem.Open();

                        Document document = projectItem.Document;
                        TextDocument textDocument = document.Object("TextDocument") as TextDocument;

                        if (textDocument == null) continue;

                        Parser parser = 
                            new Parser(projectItem, SettingsHelper.Instance.Settings, StartMenuItemPackage.ApplicationObject);
                        parser.Start(textDocument.StartPoint, textDocument.EndPoint, textDocument.EndPoint.Line);

                        foreach (StringResource resource in parser.StringResources)
                            sb.AppendLine($"Name: {resource.Name}, text: {resource.Text}, location: {resource.Location}");

                        textBox.Text = sb.ToString();

                        document.Close();               
                    }
                }
            }
            
        }
    }
}