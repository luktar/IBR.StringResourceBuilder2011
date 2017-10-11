using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface ISettingsHelper
    {
        ISettings Settings { get; }

        void Save();
    }
}
