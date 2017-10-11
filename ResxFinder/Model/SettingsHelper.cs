using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class SettingsHelper : ISettingsHelper
    {
        private bool isPathReadOnly;
        private string fileName;
        private bool isFileReadOnly;
        private ISettings settings;

        private static SettingsHelper instance;

        public ISettings Settings { get
            {
                settings = GetSettings();
                return settings;
            }
        }

        public static SettingsHelper Instance
        {
            get {
                instance = instance ?? new SettingsHelper();
                return instance;
            }
        }

        /// <summary>
        /// Singleton constructor.
        /// </summary>
        private SettingsHelper() { }

        public void Save()
        {
            if (isPathReadOnly || settings == null || isFileReadOnly) return;
            System.IO.File.WriteAllText(fileName, settings.Serialize(), Encoding.UTF8);
        }

        private ISettings GetSettings()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Debug.Print(path);

            FileSystemRights rights = CUtil.GetCurrentUsersFileSystemRights(path);
            System.Diagnostics.Debug.Print("-> {0}", rights);

            isPathReadOnly = !CUtil.Contains(rights, FileSystemRights.Write);
            fileName = System.IO.Path.Combine(path, "Settings.xml");
            isFileReadOnly = CUtil.IsFileReadOnly(fileName) || isPathReadOnly;

            if (System.IO.File.Exists(fileName))
            {
                string xml = System.IO.File.ReadAllText(fileName, Encoding.UTF8);
                return Model.Settings.DeSerialize(xml);
            }
            return new Settings();
        }
    }
}
