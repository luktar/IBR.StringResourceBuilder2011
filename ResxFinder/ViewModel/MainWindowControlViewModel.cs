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
        private bool? areAllSelected;

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        //public bool? AreAllSelected
        //{
        //    get { return AreAllSelected; }
        //    set
        //    {
        //        areAllSelected = value;

                

        //        RaisePropertyChanged(
        //            nameof(AreAllSelected));
        //    }
        //}

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

                    ISettings settings = ViewModelLocator.Instance.GetInstance<ISettingsHelper>().Settings;

                    IResourcesManager resourceManager = new ResourcesManager(
                        settings, x.Parser.ProjectItem, DocumentsManager);

                    x.Parser.StringResources.Reverse();

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
            ISettingsHelper settings = ViewModelLocator.Instance.GetInstance<ISettingsHelper>();

            SettingsWindow window = new SettingsWindow(settings.Settings);
            if (window.ShowDialog() ?? false)
            {
                settings.Save();
            }
        }   
        
        //private void UpdateCheckBoxes()
        //{
        //    if(AreAllStringResourcesChecked())
        //    {
        //        AreAllSelected = true;
        //        return;
        //    }

        //    if (AreAllStringResourcesUnchecked())
        //    {
        //        AreAllSelected = false;
        //        return;
        //    }
        //    AreAllSelected = null;
        //}

        private bool AreAllStringResourcesUnchecked()
        {
            return Parsers.ToList().Any(
                _parser => _parser.StringResources.Any(
                    _resource => _resource.IsChecked));
        }

        private bool AreAllStringResourcesChecked()
        {
            return Parsers.ToList().Any(
                _parser => _parser.StringResources.Any(
                    _resource => !_resource.IsChecked));
        }
    }
}
