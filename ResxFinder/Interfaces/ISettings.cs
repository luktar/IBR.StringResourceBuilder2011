using System.Collections.Generic;

namespace ResxFinder.Interfaces
{
    public interface ISettings
    {
        string GlobalResourceFileName { get; set; }
        List<string> IgnoreMethods { get; set; }
        List<string> IgnoreMethodsArguments { get; set; }
        int IgnoreStringLength { get; set; }
        List<string> IgnoreStrings { get; set; }
        List<string> IgnoreSubStrings { get; set; }
        bool IsDontUseResourceAlias { get; set; }
        bool IsIgnoreNumberStrings { get; set; }
        bool IsIgnoreStringLength { get; set; }
        bool IsIgnoreVerbatimStrings { get; set; }
        bool IsIgnoreWhiteSpaceStrings { get; set; }
        bool IsUseGlobalResourceFile { get; set; }

        string Serialize();
        bool IgnoreMethod(string name);

        bool IgnoreMethodArguments(string name);

        bool IgnoreString(string text);
    }
}