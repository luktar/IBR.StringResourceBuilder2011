using GalaSoft.MvvmLight;
using ResxFinder.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.ViewModel
{
    public class ParserViewModel : ViewModelBase
    {
        private string fileName;

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

        public ObservableCollection<StringResourceViewModel> StringResources { get; set; } = 
            new ObservableCollection<StringResourceViewModel>();

        public ParserViewModel(Parser parser)
        {
            Parser = parser;
            FileName = Parser.FileName;
            Parser.StringResources.ForEach(x => StringResources.Add(
                new StringResourceViewModel(x)));
        }
    }
}