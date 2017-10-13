using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ResxFinder.Messages;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.ViewModel
{
    public class StringResourceViewModel : ViewModelBase
    {
        private string text;
        private string name;
        private Point location;
        private bool isChecked;

        public StringResource StringResource { get; private set; }

        public RelayCommand IsCheckedCommand { get; private set; }

        public bool IsChecked
        {
            get { return isChecked; }
            set {
                isChecked = value;
                RaisePropertyChanged(nameof(IsChecked));
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public Point Location
        {
            get { return location; }
            private set
            {
                location = value;
                RaisePropertyChanged(nameof(Location));
            }
        }

        public StringResourceViewModel(StringResource stringResource)
        {
            IsChecked = true;
            StringResource = stringResource;

            Name = StringResource.Name;
            Text = StringResource.Text;
            Location = StringResource.Location;

            IsCheckedCommand = new RelayCommand(IsCheckedPressed);
        }

        private void IsCheckedPressed()
        {
            Messenger.Default.Send(new UpdateParserCheckBoxesMessage(StringResource.Parent));
        }

        public override string ToString()
        {
            return Name ?? "Unknown name" + ", " + Text ?? "Unknown text.";
        }
    }
}
