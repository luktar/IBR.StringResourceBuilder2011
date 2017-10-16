using ResxFinder.Model.CodeHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Interfaces
{
    public interface ICodeTools
    {
        List<CodeTextLine> GetFilteredLines(
            List<CodeTextElement> codeTextElements,
            ISettings settings);

        List<CodeTextElement> GetCodeElements(int startingLine, string text);
    }
}
