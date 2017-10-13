using EnvDTE;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface IParser
    {
        ProjectItem ProjectItem { get; }
        string FileName { get; }
        List<StringResource> StringResources { get; }
        bool Start();
    }
}
