using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvDTE;
using ResxFinder.Model;

namespace ResxFinder.Design
{
    public class DesignParser : IParser
    {
        public ProjectItem ProjectItem { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string FileName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public List<StringResource> StringResources { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public TextDocument OpenTextDocument()
        {
            return null;
        }

        public bool Start(TextPoint startPoint, TextPoint endPoint, int lastDocumentLength)
        {
            return false;
        }
    }
}
