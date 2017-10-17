using EnvDTE;
using EnvDTE80;
using NLog;
using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class DocumentsManager : IDocumentsManager
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        public Window OpenWindow(string fileName)
        {
            try
            {
                DTE2 dte = ResxFinderPackage.ApplicationObject;
                dte.MainWindow.Activate();

                return dte.ItemOperations.OpenFile(fileName, EnvDTE.Constants.vsViewKindTextView);
            }
            catch
            {
                logger.Warn("Cannot open window for file: " + fileName);
                throw;
            }
        }

        public TextDocument GetTextDocument(ProjectItem projectItem)
        {
            try
            {
                if (!projectItem.IsOpen)
                    projectItem.Open();

                Document document = projectItem.Document;

                return document.Object(Constants.TEXT_DOCUMENT) as TextDocument;
            }
            catch (Exception e)
            {
                string logMessage =
                    "Problem with obtaining TextDocument from project item: " +
                    projectItem.Properties.Item(Constants.FULL_PATH).Value;
                logger.Warn(
                    logMessage);
                throw new Exception(logMessage, e);
            }
        }

    }
}
