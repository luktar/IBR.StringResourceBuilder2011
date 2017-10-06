using GalaSoft.MvvmLight;
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

        private StringResource StringResource { get; set; }

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
            StringResource = stringResource;

            Name = StringResource.Name;
            Text = StringResource.Text;
            Location = StringResource.Location;
        }
    }
}
