using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class ItemSelectionHelper
    {
        public static void SelectStringInTextDocument(TextDocument textDocument, StringResource stringResource)
        {
            if (stringResource == null)
                return;

            string text = stringResource.Text;
            System.Drawing.Point location = stringResource.Location;

            textDocument.Selection.MoveToLineAndOffset(location.X, location.Y, false);

            if (location.Y > 1)
            {
                textDocument.Selection.MoveToLineAndOffset(location.X, location.Y - 1, false);
                textDocument.Selection.CharRight(true, 1);
                if (textDocument.Selection.Text[0] != '@')
                    textDocument.Selection.MoveToLineAndOffset(location.X, location.Y, false);
            }
            textDocument.Selection.MoveToLineAndOffset(location.X, location.Y + text.Length + 2, true);
        }
    }
}
