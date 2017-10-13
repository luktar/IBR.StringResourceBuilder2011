using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NLog;
using ResxFinder.Interfaces;
using ResxFinder.Messages;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ResxFinder.ViewModel
{
    public class ParserViewModel : ViewModelBase
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private bool? isChecked;
        private StringResourceViewModel selectedItem;
        private string fileName;

        private IDocumentsManager DocumentsManager { get; set; }

        public ICollectionView CollectionView { get; set; }

        public IParser Parser { get; private set; }

        public RelayCommand DoubleClickCommand { get; set; }

        public RelayCommand IsCheckedCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public string FileName
        {
            get { return fileName; }
            set
            {
                fileName = value;
                RaisePropertyChanged(nameof(FileName));
            }
        }

        public StringResourceViewModel SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public ObservableCollection<StringResourceViewModel> StringResources { get; set; } = 
            new ObservableCollection<StringResourceViewModel>();

        public ParserViewModel(IParser parser, IDocumentsManager documentsManager)
        {
            DocumentsManager = documentsManager;    
            Parser = parser;
            FileName = Parser.FileName;
            Parser.StringResources.ForEach(x => StringResources.Add(
                new StringResourceViewModel(x)));

            CollectionView = CollectionViewSource.GetDefaultView(StringResources);

            DoubleClickCommand = new RelayCommand(DoubleClickPressed);
            IsCheckedCommand = new RelayCommand(IsCheckedPressed);
            RefreshCommand = new RelayCommand(RefreshPressed);

            Messenger.Default.Register<UpdateParserCheckBoxesMessage>(this, UpdateCheckBoxes);
        }

        private void RefreshPressed()
        {
            StringResources.Clear();

            Parser.Start();

            if (Parser.StringResources.Count > 0)
                Parser.StringResources.ForEach(x => StringResources.Add(
                    new StringResourceViewModel(x)));

            UpdateCheckBoxes();

            Messenger.Default.Send(new UpdateParsersMessage());
        }

        #region Checkbox selection

        private void IsCheckedPressed()
        {
            if (!IsChecked.HasValue) return;
            UpdateChildsSelection(IsChecked.Value);
            Messenger.Default.Send(new UpdateTopCheckboxesMessage());
        }

        private void UpdateCheckBoxes(UpdateParserCheckBoxesMessage message)
        {
            try
            {
                if (!Parser.Equals(message.Parser)) return;
                UpdateCheckBoxes();

            } catch(Exception e)
            {
                logger.Warn(e, $"Unable to refresh selection for file parser: {FileName}.");
            }
        }

        private void UpdateCheckBoxes()
        {
            IsChecked = GetSelectionState();
            Messenger.Default.Send(new UpdateTopCheckboxesMessage());
        }

        private bool? GetSelectionState()
        {
            if (StringResources.Count == 0) return false;

            List<StringResourceViewModel> stringResources = StringResources.ToList();

            if (stringResources.All(x => x.IsChecked)) return true;
            if (stringResources.All(x => !x.IsChecked)) return false;
            return null;
        }

        public void UpdateChildsSelection(bool value)
        {
            StringResources.ToList().ForEach(x => x.IsChecked = value);
        }

        # endregion

        private void DoubleClickPressed()
        {
            try
            {
                if (SelectedItem == null) return;
          
                DocumentsManager.OpenWindow(Parser.FileName);
                TextDocument textDocument = DocumentsManager.GetTextDocument(Parser.ProjectItem);
                ItemSelectionHelper.SelectStringInTextDocument(
                    textDocument, SelectedItem.StringResource);

            } catch(Exception e)
            {
                string itemData = "Unknown element.";
                if (SelectedItem != null)
                    itemData = SelectedItem.ToString();

                logger.Warn(e, "Problem occurred while double click on element: " + 
                    itemData + ", in file: " + Parser.FileName);
            }
        }
    }
}