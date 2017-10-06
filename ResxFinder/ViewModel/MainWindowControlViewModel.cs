using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.VisualStudio;
using NLog;
using ResxFinder.Model;
using ResxFinder.Interfaces;
using ResxFinder.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.ViewModel
{
    public class MainWindowControlViewModel : ViewModelBase
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public RelayCommand PropertiesCommand { get; private set; }
        public RelayCommand RunCommand { get; private set; }

        public ObservableCollection<ParserViewModel> Parsers { get; set; } =
            new ObservableCollection<ParserViewModel>();

        public MainWindowControlViewModel()
        {
            PropertiesCommand = new RelayCommand(PropertiesPressed);
            RunCommand = new RelayCommand(RunPressed);

        }

        private void RunPressed()
        {
            ISolutionProjectsHelper solutionProjectHelper = 
                ViewModelLocator.Instance.GetInstance<ISolutionProjectsHelper>();

            List<Project> projects = solutionProjectHelper.GetProjects();

            IParserManager parserManager = 
                ViewModelLocator.Instance.GetInstance<IParserManager>();

            List<Parser> parsers = parserManager.GetParsers(projects);

            Parsers.Clear();
            parsers.ForEach(x => Parsers.Add(new ParserViewModel(x)));
        }

        private void PropertiesPressed()
        {
            SettingsWindow window = new SettingsWindow(SettingsHelper.Instance.Settings);
            if (window.ShowDialog() ?? false)
            {
                SettingsHelper.Instance.Save();
            }
        }     
    }
}
