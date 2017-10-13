using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model.CodeHelpers
{
    public class CodeTextLine
    {
        public int LineNumber { get; private set; }
        public string LineText { get; private set; }

        public CodeTextLine(int lineNumber, string lineText)
        {
            LineNumber = lineNumber;
            LineText = lineText;
        }
    }
}
