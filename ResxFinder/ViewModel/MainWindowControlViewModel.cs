using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using ResxFinder.Interfaces;
using ResxFinder.Messages;
using ResxFinder.Model;
using ResxFinder.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ResxFinder.ViewModel
{
    public class MainWindowControlViewModel : ViewModelBase
    {
        private string message;

        private bool? isChecked;

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public bool? IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public RelayCommand IsCheckedCommand { get; private set; }

        public RelayCommand PropertiesCommand { get; private set; }

        public RelayCommand RunCommand { get; private set; }

        public RelayCommand MoveToResourcesCommand { get; private set; }

        public ObservableCollection<ParserViewModel> Parsers { get; set; } =
            new ObservableCollection<ParserViewModel>();

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }

        private IDocumentsManager DocumentsManager { get; set; }

        public MainWindowControlViewModel()
        {
            IsChecked = false;

            PropertiesCommand = new RelayCommand(PropertiesPressed);
            RunCommand = new RelayCommand(RunPressed, CanRunCommand);
            MoveToResourcesCommand = new RelayCommand(MoveToResourcesPressed, CanMoveToResources);
            IsCheckedCommand = new RelayCommand(IsCheckedPressed, IsCheckedEnabled);

            DocumentsManager = ViewModelLocator.Instance.GetInstance<IDocumentsManager>();

            Messenger.Default.Register<UpdateTopCheckboxesMessage>(this, UpdateCheckboxes);
            Messenger.Default.Register<UpdateParsersMessage>(this, UpdateParsers);
            Messenger.Default.Register<UpdateProgressMessage>(this, UpdateProgress);

            ResxFinderPackage.ApplicationObject.Events.SolutionEvents.Opened += SolutionEvents_Opened;
            ResxFinderPackage.ApplicationObject.Events.SolutionEvents.AfterClosing += SolutionEvents_Closed;

            if (!ResxFinderPackage.ApplicationObject.Solution.IsOpen)
            {
                SolutionClosedMessage();
            }
        }
        private void SolutionEvents_Closed()
        {
            IsChecked = false;
            Parsers.Clear();

            SolutionClosedMessage();

            RunCommand.RaiseCanExecuteChanged();
            MoveToResourcesCommand.RaiseCanExecuteChanged();
            IsCheckedCommand.RaiseCanExecuteChanged();
        }
        private bool CanRunCommand()
        {
            return ResxFinderPackage.ApplicationObject.Solution.IsOpen;
        }

        private void SolutionClosedMessage()
        {
            Message = "No solution loaded.";
        }

        private void SolutionEvents_Opened()
        {
            IsChecked = false;
            Parsers.Clear();

            Message = "Solution active. Press \"Find hard coded strings\" button.";

            RunCommand.RaiseCanExecuteChanged();
            MoveToResourcesCommand.RaiseCanExecuteChanged();
            IsCheckedCommand.RaiseCanExecuteChanged();
        }

        #region Checkbox selection

        private bool IsCheckedEnabled()
        {
            return Parsers.Count > 0;
        }

        private void IsCheckedPressed()
        {
            if (!IsChecked.HasValue) return;
            UpdateChildsSelection(IsChecked.Value);
        }

        private void UpdateCheckboxes(UpdateTopCheckboxesMessage message)
        {
            try
            {
                IsChecked = GetSelectionState();

            }
            catch (Exception e)
            {
                logger.Warn(e, $"Problem with selection parsers from top level.");
            }
        }

        private bool? GetSelectionState()
        {
            List<ParserViewModel> parsers = Parsers.ToList();
            if (parsers.All(x => x.IsChecked == true)) return true;
            if (parsers.All(x => x.IsChecked == false)) return false;
            return null;
        }

        private void UpdateChildsSelection(bool value)
        {
            Parsers.ToList().ForEach(x =>
            {
                x.IsChecked = value;
                x.UpdateChildsSelection(value);
            });
        }

        # endregion

        private bool CanMoveToResources()
        {
            return Parsers.Count > 0;
        }

        private void MoveToResourcesPressed()
        {
            Message = "Moving hard coded string to resources.";
            logger.Debug("Moving hard coded strings to resource file.");

            string projectFileName = string.Empty;
            string currentStringResource = null;

            try
            {
                ISettings settings = ViewModelLocator.Instance.GetInstance<ISettingsHelper>().Settings;

                foreach (ParserViewModel parser in Parsers.ToList())
                {
                    if (parser.IsChecked == false) continue;

                    projectFileName = parser.FileName;

                    IResourcesManager resourceManager = new ResourcesManager(
                        settings, parser.Parser.ProjectItem, DocumentsManager);

                    List<StringResourceViewModel> stringResources = parser.StringResources.ToList();
                    stringResources.Reverse();

                    stringResources.ForEach(y =>
                    {
                        if (y.IsChecked)
                        {
                            currentStringResource = y.StringResource.ToString();

                            try
                            {
                                resourceManager.WriteToResource(y.StringResource);
                            }
                            catch (Exception e)
                            {
                                logger.Warn(e, $"Problem with writing string to resource. Resource details: {currentStringResource}.");
                            }
                        }

                    });

                    resourceManager.Finish();

                };
                Message = "Operation completed successfully.";
            }
            catch (Exception e)
            {
                if (currentStringResource == null) currentStringResource = "Unknown";
                logger.Error(e,
                    $"Unable to move hard coded strings to resource file. Project file name: {projectFileName}, resource: {currentStringResource}");
            }
        }

        private void RunPressed()
        {
            Message = "Search started.";

            string currentParserName = string.Empty;
            try
            {
                ISolutionProjectsHelper solutionProjectHelper =
                    ViewModelLocator.Instance.GetInstance<ISolutionProjectsHelper>();

                ISettings settings = ViewModelLocator.Instance.GetInstance<ISettingsHelper>().Settings;

                List<Project> projects = solutionProjectHelper.GetProjects(settings);

                IParserManager parserManager =
                    ViewModelLocator.Instance.GetInstance<IParserManager>();

                List<IParser> parsers = parserManager.GetParsers(projects);

                Parsers.Clear();
                parsers.ForEach(x =>
                {
                    currentParserName = x.FileName;
                    Parsers.Add(new ParserViewModel(x, DocumentsManager));
                });

                UpdateSelection();
                MoveToResourcesCommand.RaiseCanExecuteChanged();

                Message = "Search completed.";
            }
            catch (Exception e)
            {
                logger.Error(e,
                    $"An error occurred while parsing all projects in solution. File name: {currentParserName}.");
            }
        }

        public void UpdateProgress(UpdateProgressMessage message)
        {
            // TODO: Fix refreshing data.
            Application.Current.Dispatcher.Invoke(() =>
            {
                Message = "Loading: " + message.Current + " / " + message.Total;
            }, DispatcherPriority.Send);


        }

        private void UpdateSelection()
        {
            if (Parsers.Count > 0)
            {
                IsChecked = true;
                UpdateChildsSelection(true);
            }
            else
            {
                IsChecked = false;
            }
            IsCheckedCommand.RaiseCanExecuteChanged();
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

        private void UpdateParsers(UpdateParsersMessage parserMessage)
        {
            List<ParserViewModel> parsersToRemove = Parsers.Where(x => x.StringResources.Count == 0).ToList();
            parsersToRemove.ForEach(x => Parsers.Remove(x));

            IsChecked = GetSelectionState();
            MoveToResourcesCommand.RaiseCanExecuteChanged();

        }
    }
}
