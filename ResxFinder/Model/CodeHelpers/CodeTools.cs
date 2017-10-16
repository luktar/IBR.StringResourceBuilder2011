using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ResxFinder.Model.CodeHelpers
{
    public class CodeTools : ICodeTools
    {
        private const string END_LINE_PATTERN = "(\\s*;\\s*(\\/\\/.*)*)$";
        private const string END_ATTRIBUTE_PATTERN = "(]\\s*)$";

        private Regex EndLineRegex { get; } =
            new Regex(END_LINE_PATTERN);

        private Regex EndAttributeRegex { get; } =
            new Regex(END_ATTRIBUTE_PATTERN);

        public List<CodeTextLine> GetFilteredLines(
            List<CodeTextElement> codeTextElements,
            ISettings settings)
        {
            List<CodeTextLine> result = new List<CodeTextLine>();

            foreach(CodeTextElement element in codeTextElements)
            {
                if (!element.IsMatching(settings))
                    result.AddRange(element.Lines);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Dictionary where key is starting line number, value is code block text.</returns>
        public List<CodeTextElement> GetCodeElements(int startingLine, string text)
        {
            List<CodeTextElement> result = new List<CodeTextElement>();

            List<CodeTextLine> codeLines = new List<CodeTextLine>();

            int currentLine = startingLine;

            using (StringReader reader = new StringReader(text))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if(!IsLineBlockEnd(line))
                        {
                            codeLines.Add(new CodeTextLine(currentLine, line));
                        } else
                        {
                            if(codeLines.Count == 0)
                                result.Add(
                                    new CodeTextElement(
                                        new CodeTextLine(currentLine, line)));
                            else
                            {
                                codeLines.Add(new CodeTextLine(currentLine, line));
                                result.Add(new CodeTextElement(codeLines));
                                codeLines.Clear();
                            }
                        }
                    }
                    ++currentLine;
                } while (line != null);
            }

            return result;
        }

        private bool IsLineBlockEnd(string codeLine)
        {
            return 
                EndLineRegex.Match(codeLine).Success ||
                EndAttributeRegex.Match(codeLine).Success;
        }
    }
}
