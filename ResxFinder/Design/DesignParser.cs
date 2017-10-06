using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;

namespace ResxFinder.Design
{
    public class DesignParser : IParser
    {
        public TextDocument GetTextDocument()
        {
            return null;
        }

        public bool Start(TextPoint startPoint, TextPoint endPoint, int lastDocumentLength)
        {
            return false;
        }
    }
}
