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

            ParserManager parserManager = new ParserManager();
            parserManager.Start(solutionProjectHelper.GetProjects());

            WriteReport(Parsers);
        }

        private void PropertiesPressed()
        {
            SettingsWindow window = new SettingsWindow(SettingsHelper.Instance.Settings);
            if (window.ShowDialog() ?? false)
            {
                SettingsHelper.Instance.Save();
            }
        }

        private void WriteReport(List<Parser> parsers)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Parser parser in parsers)
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

        
    }
}
