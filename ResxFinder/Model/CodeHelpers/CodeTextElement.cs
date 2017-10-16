using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model.CodeHelpers
{
    public class CodeTextElement
    {
        public int StartLine { get; private set; }

        public int EndLine { get; private set; }

        public List<CodeTextLine> Lines { get; private set; } =
            new List<CodeTextLine>();

        public string Text { get; private set; }

        public CodeTextElement(List<CodeTextLine> lines)
        {
            Lines.AddRange(lines);

            StartLine = Lines.First().LineNumber;
            EndLine = Lines.Last().LineNumber;

            Text = ConcatString(Lines);
        }

        public CodeTextElement(CodeTextLine element) : this(new List<CodeTextLine>() { element })
        {

        }

        private static string ConcatString(List<CodeTextLine> codeLines)
        {
            StringBuilder sb = new StringBuilder();
            codeLines.ForEach(x => sb.AppendLine(x.LineText));
            return sb.ToString();
        }

        public bool ContainsText(List<string> texts)
        {
            if (texts.Count == 0) return false;

            foreach (string text in texts)
                if (Text.Contains(text)) return true;
            return false;
        }

        public bool IsMatching(ISettings settings)
        {
            return settings.TextMatch(Text);
        }

        public bool ContainsText(string text)
        {
            return Text.Contains(text);
        }
    }
}
