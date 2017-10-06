using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
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
            
        }
    }
}