﻿using EnvDTE;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface IParserManager
    {
        List<IParser> GetParsers(List<Project> projects);
    }
}
