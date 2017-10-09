using EnvDTE;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using NLog;
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

        private StringResourceViewModel selectedItem;
        private string fileName;

        public ICollectionView CollectionView { get; set; }

        public RelayCommand DoubleClickCommand { get; set; }

        private Parser Parser { get; set; }

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

        public List<StringResourceViewModel> StringResources { get; set; } = 
            new List<StringResourceViewModel>();

        public ParserViewModel(Parser parser)
        {
            Parser = parser;
            FileName = Parser.FileName;
            Parser.StringResources.ForEach(x => StringResources.Add(
                new StringResourceViewModel(x)));

            CollectionView = CollectionViewSource.GetDefaultView(StringResources);

            DoubleClickCommand = new RelayCommand(DoubleClickPressed);
        }

        private void DoubleClickPressed()
        {
            try
            {
                if (SelectedItem == null) return;

                DocumentsManager documentsManager = new DocumentsManager();
                documentsManager.OpenWindow(Parser.FileName);
                TextDocument textDocument = documentsManager.GetTextDocument(Parser.ProjectItem);
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