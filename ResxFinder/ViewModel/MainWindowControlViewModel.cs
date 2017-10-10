using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
using ResxFinder.Model;
using ResxFinder.Interfaces;
using ResxFinder.Views;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Linq;

namespace ResxFinder.ViewModel
{
    public class MainWindowControlViewModel : ViewModelBase
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public RelayCommand PropertiesCommand { get; private set; }

        public RelayCommand RunCommand { get; private set; }

        public RelayCommand MoveToResourcesCommand { get; private set; }

        public ObservableCollection<ParserViewModel> Parsers { get; set; } =
            new ObservableCollection<ParserViewModel>();

        private IDocumentsManager DocumentsManager { get; set; }

        public MainWindowControlViewModel()
        {
            PropertiesCommand = new RelayCommand(PropertiesPressed);
            RunCommand = new RelayCommand(RunPressed);
            MoveToResourcesCommand = new RelayCommand(MoveToResourcesPressed, CanMoveToResources);

            DocumentsManager = ViewModelLocator.Instance.GetInstance<IDocumentsManager>();
        }

        private bool CanMoveToResources()
        {
            return Parsers.Count > 0;
        }

        private void MoveToResourcesPressed()
        {
            logger.Debug("Moving hard coded strings to resource file.");

            string projectFileName = string.Empty;
            string currentStringResource = null;

            try
            {
                Parsers.ToList().ForEach(x =>
                {
                    projectFileName = x.FileName;

                    IResourcesManager resourceManager = new ResourcesManager(
                        SettingsHelper.Instance.Settings, x.Parser.ProjectItem, DocumentsManager);

                    x.Parser.StringResources.ForEach(y =>
                    {
                        currentStringResource = y.ToString();

                        resourceManager.WriteToResource(y);
                  
                    });
                    resourceManager.InsertNamespace();

                });
            } catch(Exception e)
            {
                if (currentStringResource == null) currentStringResource = "Unknown";
                logger.Error(e, 
                    $"Unable to move hard coded strings to resource file. Project file name: {projectFileName}, resource: {currentStringResource}");
            }
        }

        private void RunPressed()
        {
            string currentParserName = string.Empty;
            try
            {
                ISolutionProjectsHelper solutionProjectHelper =
                    ViewModelLocator.Instance.GetInstance<ISolutionProjectsHelper>();

                List<Project> projects = solutionProjectHelper.GetProjects();

                IParserManager parserManager =
                    ViewModelLocator.Instance.GetInstance<IParserManager>();

                List<IParser> parsers = parserManager.GetParsers(projects);

                Parsers.Clear();
                parsers.ForEach(x => {
                    currentParserName = x.FileName;
                    Parsers.Add(new ParserViewModel(x, DocumentsManager));
                });

                MoveToResourcesCommand.RaiseCanExecuteChanged();
            } catch (Exception e)
            {
                logger.Error(e, 
                    $"An error occurred while parsing all projects in solution. File name: {currentParserName}.");
            }
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
