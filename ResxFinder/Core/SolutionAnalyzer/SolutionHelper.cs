using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ResxFinder.Core.SolutionAnalyzer
{
    public class SolutionHelper : ISolutionHelper
    {
        public Solution GetSolution()
        {
            return StartMenuItemPackage.ApplicationObject.Solution;
        }
    }
}
