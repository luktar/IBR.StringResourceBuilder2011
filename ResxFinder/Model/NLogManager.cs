using NLog;
using NLog.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model
{
    public class NLogManager
    {
        public static readonly LogFactory Instance = 
            new LogFactory(new XmlLoggingConfiguration(GetNLogConfigFilePath()));

        private static string GetNLogConfigFilePath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            string assemblyDirectory = Path.GetDirectoryName(path);
            return Path.Combine(assemblyDirectory, "NLog.config");
        }
    }
}
