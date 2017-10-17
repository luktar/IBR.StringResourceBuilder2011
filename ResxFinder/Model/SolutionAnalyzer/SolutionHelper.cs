﻿using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ResxFinder.Model.SolutionAnalyzer
{
    public class SolutionHelper : ISolutionHelper
    {
        public Solution GetSolution()
        {
            return ResxFinderPackage.ApplicationObject.Solution;
        }
    }
}
