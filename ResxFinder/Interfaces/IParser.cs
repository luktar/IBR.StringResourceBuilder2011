﻿using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface IParser
    {
        bool Start(TextPoint startPoint, TextPoint endPoint, int lastDocumentLength);
    }
}
