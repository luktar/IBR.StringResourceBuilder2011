using EnvDTE;
using ResxFinder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface IParserManager
    {
        List<Parser> GetParsers(List<Project> projects);
    }
}
