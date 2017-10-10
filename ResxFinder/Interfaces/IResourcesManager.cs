using EnvDTE;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface IResourcesManager
    {
        void WriteToResource(StringResource stringResource);

        void InsertNamespace();
    }
}
