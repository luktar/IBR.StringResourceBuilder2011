using System;
using System.Collections.Generic;
using System.Text;
using EnvDTE;
using EnvDTE80;
using NLog;
using ResxFinder.Interfaces;

namespace ResxFinder.Model
{
    public class Parser : IParser
    {
        private const string FULL_PATH = "FullPath";

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private DTE2 dte;   
        private ISettings m_Settings;
        private bool isCsharp;
        public ProjectItem ProjectItem { get; private set; }

        public string FileName { get; private set; }

        public List<StringResource> StringResources { get; private set; } = new List<StringResource>();

        public Parser(ProjectItem projectItem,
                            ISettings settings,
                            DTE2 dte)
        {
            this.ProjectItem = projectItem;
            isCsharp = true;
            FileName = projectItem.Properties.Item(FULL_PATH).Value.ToString();
            this.dte = dte;
            m_Settings = settings;
        }

        public bool Start(TextPoint startPoint,
                                     TextPoint endPoint,
                                     int lastDocumentLength)
        {
            try
            {
                logger.Debug("Start - parsing document: " + FileName);

                List<StringResource> stringResources = new List<StringResource>();

                bool isFullDocument = startPoint.AtStartOfDocument && endPoint.AtEndOfDocument,
                     isTextWithStringLiteral = true;
                int startLine = startPoint.Line,
                    startCol = startPoint.LineCharOffset,
                    endLine = endPoint.Line,
                    endCol = endPoint.LineCharOffset,
                    documentLength = endPoint.Parent.EndPoint.Line,
                    insertIndex = 0;

                if (isFullDocument)
                    StringResources.Clear();
                else
                {
                    //determine whether the text between startLine and endLine (including) contains double quotes
                    EditPoint editPoint = startPoint.CreateEditPoint() as EditPoint2;
                    if (!startPoint.AtStartOfLine)
                        editPoint.StartOfLine();
                    isTextWithStringLiteral = editPoint.GetLines(startLine, endLine + 1).Contains("\"");

                    //move trailing locations behind changed lines if needed and
                    //remove string resources on changed lines
                    int lineOffset = documentLength - lastDocumentLength;

                    for (int i = StringResources.Count - 1; i >= 0; --i)
                    {
                        StringResource stringResource = StringResources[i];
                        int lineNo = stringResource.Location.X;

                        if (lineNo + lineOffset > endLine)
                        {
                            if (lineOffset != 0)
                            {
                                stringResource.Offset(lineOffset, 0); //move
                            }
                        }
                        else if (lineNo >= startLine)
                        {
                            StringResources.RemoveAt(i); //remove changed line
                        }
                        else if (insertIndex == 0)
                        {
                            insertIndex = i + 1;
                        }
                    }
                }

                if (isTextWithStringLiteral)
                {
                    CodeElements elements = ProjectItem.FileCodeModel.CodeElements;

                    foreach (CodeElement element in elements)
                    {
                        ParseForStrings(element, stringResources, isCsharp, m_Settings, startLine, endLine);
                    }

                    if (isFullDocument)
                        StringResources.AddRange(stringResources);
                    else if (stringResources.Count > 0)
                        StringResources.InsertRange(insertIndex, stringResources);
                }

                logger.Debug("End - parsing document: " + FileName);

                return true;
            } catch(Exception e)
            {
                logger.Warn(e, "An error occurred while parsing file: " + FileName);
                return false;
            }
        }

        private static bool IsElementCorrect(CodeElement element, int startLine,
                                        int endLine, bool isCSharp)
        {
            if (element == null)
                return false;

            try
            {
                if (element.StartPoint.Line > endLine)
                    return false;
            }
            catch (Exception ex)
            {
                //element.StartPoint not implemented in VS2017 15.2 (26430.6) for expression bodied property getters (before no getter element was available)
                System.Diagnostics.Debug.Print("### Error: ParseForStrings({0}): element.StartPoint.Line > endLine? {1} - {2}", element.Kind, ex.GetType().Name, ex.Message);
                return false;
            }

            try
            {
                if (element.EndPoint.Line < startLine)
                    return false;
            }
            catch (Exception ex)
            {
                //element.EndPoint invalid when deleting or cutting text
                System.Diagnostics.Debug.Print("### Error: ParseForStrings(): element.EndPoint < startLine? {0} - {1}", ex.GetType().Name, ex.Message);
                return false;
            }

            return true;
        }

        private static bool IsClassOrStruct(CodeElement element)
        {
            return element.IsCodeType
                && ((element.Kind == vsCMElement.vsCMElementClass)
                    || (element.Kind == vsCMElement.vsCMElementStruct));
        }

        /// <summary>Parses for strings by evaluating the element kind.</summary>
        /// <param name="element">The element.</param>
        /// <param name="progressWorker">The progress worker.</param>
        /// <param name="stringResources">The string resources.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="isCSharp">If set to <c>true</c> it is CSharp code.</param>
        /// <param name="startLine">The start line.</param>
        /// <param name="endLine">The end line.</param>
        private static void ParseForStrings(CodeElement element,
                                        List<StringResource> stringResources,
                                        bool isCSharp,
                                        ISettings settings,
                                        int startLine,
                                        int endLine)
        {
            if (!IsElementCorrect(element, startLine, endLine, isCSharp)) return;

            if (IsClassOrStruct(element))
            {
                AnalyzeCalssOrStructure(element, stringResources, isCSharp, settings, startLine, endLine);
            }
            else if (element.Kind == vsCMElement.vsCMElementNamespace)
            {
                AnalyzeNamespace(element, stringResources, isCSharp, settings, startLine, endLine);
            }
            else if (element.Kind == vsCMElement.vsCMElementProperty)
            {
                AnalyzeProperty(element, stringResources, isCSharp, settings, startLine, endLine);
            }
            else if ((element.Kind == vsCMElement.vsCMElementFunction)
                     || (element.Kind == vsCMElement.vsCMElementVariable))
            {
                if ((element.Kind == vsCMElement.vsCMElementFunction) && settings.IgnoreMethod(element.Name))
                    return;

                ParseForStrings(element, stringResources, settings, isCSharp, startLine, endLine);
            }
        }

        private static void AnalyzeCalssOrStructure(CodeElement element, List<StringResource> stringResources, bool isCSharp, ISettings settings, int startLine, int endLine)
        {
            CodeElements elements = (element as CodeType).Members;
            foreach (CodeElement subElement in elements)
                ParseForStrings(subElement, stringResources, isCSharp, settings, startLine, endLine);
        }

        private static void AnalyzeNamespace(CodeElement element, List<StringResource> stringResources, bool isCSharp, ISettings settings, int startLine, int endLine)
        {
            CodeElements elements = (element as CodeNamespace).Members;

            foreach (CodeElement subElement in elements)
                ParseForStrings(subElement, stringResources, isCSharp, settings, startLine, endLine);
        }

        private static void AnalyzeProperty(CodeElement element, List<StringResource> stringResources, bool isCSharp, ISettings settings, int startLine, int endLine)
        {
            CodeProperty prop = element as CodeProperty;

            //CodeElement.StartPoint not implemented in VS2017 15.2 (26430.6) for expression bodied property getters
            //because before the expression bodied properties had Getter and Setter == null
            bool getterHasStartPoint = (prop.Getter != null) && (prop.Getter as CodeElement).HasStartPoint(), setterHasStartPoint = (prop.Setter != null) && (prop.Setter as CodeElement).HasStartPoint();

            if (getterHasStartPoint)
                ParseForStrings(prop.Getter as CodeElement, stringResources, isCSharp, settings, startLine, endLine);

            if (setterHasStartPoint)
                ParseForStrings(prop.Setter as CodeElement, stringResources, isCSharp, settings, startLine, endLine);

            if (!getterHasStartPoint && !setterHasStartPoint)
            {
                //expression bodied property
                int lineNo = ParseForStrings(element, stringResources, settings, isCSharp, startLine, endLine);
            }
        }

        /// <summary>Parses for strings by iterating through the text lines.</summary>
        /// <param name="element">The element.</param>
        /// <param name="stringResources">The string resources.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="isCSharp">If set to <c>true</c> it is CSharp code.</param>
        /// <param name="startLine">The start line.</param>
        /// <param name="endLine">The end line.</param>
        /// <returns>The last parsed line number or -1.</returns>
        private static int ParseForStrings(CodeElement element,
                                           List<StringResource> stringResources,
                                           ISettings settings,
                                           bool isCSharp,
                                           int startLine,
                                           int endLine)
        {
            TextPoint startPoint = element.StartPoint,
                       endPoint = element.EndPoint;
            EditPoint2 editPoint = null;

            try
            {
                if (element.Kind == vsCMElement.vsCMElementFunction)
                {
                    try
                    {
                        //we want to have the body only (throws COMException when inspecting an expression bodied function)
                        startPoint = element.GetStartPoint(vsCMPart.vsCMPartBody);
                        endPoint = element.GetEndPoint(vsCMPart.vsCMPartBody);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Print("ParseForStrings(vsCMElementFunction, line {0}): {1} - {2}", startPoint.Line, ex.GetType().Name, ex.Message);
                    }
                }

                editPoint = startPoint.CreateEditPoint() as EditPoint2;

                int editLine = editPoint.Line,
                         editColumn = editPoint.LineCharOffset;
                string text = editPoint.GetText(endPoint);

                if (text.Contains("@\"")) return -1;
                if (text.ToLower().Contains("string const")) return -1;

                string[] txtLines = text.Replace("\r", string.Empty).Split('\n');
                bool isComment = false;

                foreach (string txtLine in txtLines)
                {
                    if ((editLine >= startLine) && (editLine <= endLine))
                    {
                        //this is a changed text line in the block
                        if (txtLine.Contains("\""))
                            ParseForStrings(txtLine, editLine, editColumn, stringResources, settings, isCSharp, ref isComment);
                    }

                    ++editLine;

                    //only for the first line of the text block LineCharOffset will be used
                    if (editColumn > 1)
                        editColumn = 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("### Error: ParseForStrings(): {0} - {1}", ex.GetType().Name, ex.Message);
            }

            return (endPoint?.Line ?? (-1));
        }

        /// <summary>Parses for strings by iterating through the parts between double quotes.</summary>
        /// <param name="txtLine">The text line.</param>
        /// <param name="lineNo">The line number.</param>
        /// <param name="colNo">The column number.</param>
        /// <param name="stringResources">The string resources.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="isCSharp">If set to <c>true</c> it is CSharp code.</param>
        /// <param name="isComment">If set to <c>true</c> it starts (in) or ends (out) with a comment.</param>
        private static void ParseForStrings(string txtLine,
                                            int lineNo,
                                            int colNo,
                                            List<StringResource> stringResources,
                                            ISettings settings,
                                            bool isCSharp,
                                            ref bool isComment)
        {
            List<int> stringPos = new List<int>();

            bool isInString = false,
                 isAtString = false, // @"..."
                 isInterpolatedString = false; // $"..."
            StringBuilder txt = new StringBuilder();
            int pos = 0;

            string[] parts = txtLine.Split('"');

            for (int i = 0; i < parts.Length; ++i)
            {
                string part = parts[i];

                bool isEmptyPart = (part.Length == 0);

                if (!isInString)
                {
                    if (isCSharp)
                    {
                        part = HandleComment(part, ref isComment);

                        if (part.Contains("//")) //line comment -> ignore the rest of the parts
                            break;
                    }
                    else
                    {
                        if (part.Contains("'")) //line comment -> ignore the rest of the parts
                            break;

                        //REM line comment -> ignore the rest of the parts
                        // <whitespace|nonletter>REM<whitespace|nonletter>
                        // <whitespace|nonletter>REM ccc
                        int remPos = part.ToUpper().IndexOf("REM");
                        if (remPos > -1)
                        {
                            char c = (remPos + 3 < part.Length) ? part[remPos + 3] : '\0';
                            if ((c == '\0') || !(char.IsLetterOrDigit(c) || (c == '_')))
                            {
                                c = (remPos > 0) ? part[remPos - 1] : '\0';
                                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                                    break;
                            }
                        }
                    }

                    pos += part.Length + 1;

                    if (isComment)
                        continue;

                    if (isCSharp && part.EndsWith("'")) //begin of char '"'
                        continue;

                    isAtString = !isCSharp || part.EndsWith("@");
                    isInterpolatedString = isCSharp && (part.EndsWith("$") || part.EndsWith("$@"));

                    stringPos.Add(pos);

                    isInString = true;
                }
                else
                {
                    if (isInterpolatedString)
                    {
                        //SkipInterpolatedString
                        if (isAtString && isEmptyPart)
                        {
                            //handle leading double quotes
                            isInString = SkipDoubleQuotes(txtLine, ref pos, ref i);
                            continue;
                        }

                        if (!isAtString || !isEmptyPart)
                            pos += part.Length + 1;

                        if (part.Contains("{{"))
                            part = part.Replace("{{", "##");
                        if (part.Contains("}}"))
                            part = part.Replace("}}", "##");
                        if (part.Contains(@"\\"))
                            part = part.Replace(@"\\", "##");

                        int openCurlyBraces = IterateCurlyBraceBlocks(part);

                        if (openCurlyBraces > 0)
                        {
                            break;
                        }

                        if (isAtString)
                            isInString = !SkipDoubleQuotes(txtLine, ref pos, ref i); //handle trailing double quotes (inverted result)
                        else if (!part.EndsWith(@"\"))
                            isInString = false;
                    }
                    else if (isAtString)
                    {
                        if (!isEmptyPart)
                        {
                            if (!settings.IsIgnoreVerbatimStrings)
                                txt.Append(part);
                            pos += part.Length;
                        }

                        //here double quotes are given as pairs
                        int count = CountDoubleQuotes(txtLine, pos);

                        if (count > 1)
                        {
                            //skip empty parts from double quote pairs
                            i += count - 1;

                            while (count >= 2)
                            {
                                if (!settings.IsIgnoreVerbatimStrings)
                                    txt.Append("\"\"");
                                count -= 2;
                                pos += 2;
                            }
                        }

                        if (count == 1)
                        {
                            isInString = false;
                            ++pos;
                        }
                    }
                    else
                    {
                        //keep string as it is in the editor
                        pos += part.Length + 1;

                        //replace double backslash and undo it later to find single ending backslash
                        if (part.Contains(@"\\"))
                            part = part.Replace(@"\\", "\001");

                        //single ending backslash escaping a double quote character
                        if (part.EndsWith(@"\"))
                            part = part/*.Remove(part.Length - 1)*/ + "\"";
                        else
                            isInString = false;


                        if (part.Contains("\001"))
                            part = part.Replace("\001", @"\\");

                        txt.Append(part);
                    }

                    if (!isInString && (!isAtString || !settings.IsIgnoreVerbatimStrings))
                        txt.Append("\0");
                }
            }

            if (txt.Length > 0)
            {
                if (txt.ToString().EndsWith("\0"))
                    txt.Remove(txt.Length - 1, 1);

                parts = txt.ToString().Split('\0');
                for (int i = 0; i < parts.Length; ++i)
                {
                    string draftName = parts[i],
                           name = string.Empty;

                    if (settings.IgnoreString(draftName))
                        continue;

                    for (int c = 0; c < draftName.Length; ++c)
                    {
                        if (char.IsWhiteSpace(draftName[c]))
                            name += '_';
                        else if (char.IsLetterOrDigit(draftName[c]) || (draftName[c] == '_'))
                            name += draftName[c];
                    }

                    if (name.Length == 0)
                        name = stringResources.Count.ToString();

                    if (char.IsDigit(name[0]))
                        name = "_" + name;

                    stringResources.Add(new StringResource(name, draftName, new System.Drawing.Point(lineNo, stringPos[i] + colNo - 1)));
                }
            }
        }

        /// <summary>Skips the double quotes.</summary>
        /// <param name="txtLine">The text line.</param>
        /// <param name="pos">The position.</param>
        /// <param name="i">The i.</param>
        /// <returns>
        /// <c>true</c> when the number of found double qoutes is even, meaning we are still within a string;
        /// otherwise <c>false</c>.
        /// </returns>
        private static bool SkipDoubleQuotes(string txtLine,
                                             ref int pos,
                                             ref int i)
        {
            //skip empty parts from double quote pairs:
            //"x"     = true(0),  -> never occurs here (called only when part is empty)
            //""      = false(1),
            //"""x"   = true(2),
            //""""    = false(3),
            //"""""x" = true(4)

            int count = CountDoubleQuotes(txtLine, pos);

            if (count > 0)
            {
                i += count - 1;

                pos += count;
            }

            return ((count & 1) == 0); //isInString
        }

        /// <summary>Counts the consecutive double quotes.</summary>
        /// <param name="txtLine">The text line.</param>
        /// <param name="startPos">The start position.</param>
        /// <returns>The number of consecutive double quotes.</returns>
        private static int CountDoubleQuotes(string txtLine,
                                             int startPos)
        {
            int count = startPos;

            while ((count < txtLine.Length) && (txtLine[count] == '"'))
                ++count;

            count -= startPos;

            return (count);
        }

        /// <summary>Iterates through the curly brace blocks.</summary>
        /// <param name="part">The part.</param>
        /// <returns>The number of unclosed (still open) curly brace blocks.</returns>
        private static int IterateCurlyBraceBlocks(string part)
        {
            int openCurlyBraces = 0;

            for (int j = 0; j < part.Length; ++j)
            {
                char c = part[j];

                if (c == '{')
                    ++openCurlyBraces;
                else if (c == '}')
                    --openCurlyBraces;
            }

            return (openCurlyBraces);
        }

        private static string HandleComment(string txtPart,
                                            ref bool isComment)
        {
            int pos = -1;

            if (isComment)
            {
                //replace all characters of the comment by blank spaces
                pos = txtPart.IndexOf("*/");
                if (pos == -1)
                    return (new string(' ', txtPart.Length)); //no end yet

                isComment = false;
                txtPart = new string(' ', pos + 2) + txtPart.Substring(pos + 2);
            }

            int pos2 = txtPart.IndexOf("//");
            pos = txtPart.IndexOf("/*");
            if ((pos == -1) || ((pos2 > -1) && (pos > pos2)))
                return (txtPart);

            isComment = true;
            txtPart = txtPart.Substring(0, pos) + HandleComment(txtPart.Substring(pos), ref isComment);
            return (txtPart);
        }
    }
}
