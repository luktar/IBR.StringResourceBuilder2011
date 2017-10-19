using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using EnvDTE80;
using EnvDTE;
using ResxFinder.ViewModel;
using System.IO;
using System.Reflection;

namespace ResxFinder
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(ResxFinderPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideToolWindow(typeof(MainWindow))]
    public sealed class ResxFinderPackage : Package
    {
        private static ResxFinderPackage instance;

        public static DTE2 ApplicationObject { get; private set; }

        /// <summary>
        /// StartMenuItemPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "0fae8e85-b18c-47a9-a287-bdc91308f01f";

        /// <summary>
        /// Initializes a new instance of the <see cref="ResxFinderMenuItem"/> class.
        /// </summary>
        public ResxFinderPackage()
        {
            
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members
        /// <summary>
        /// https://stackoverflow.com/questions/3874015/subscription-to-dte-events-doesnt-seem-to-work-events-dont-get-called
        /// </summary>
        private static SolutionEvents solutionEvents;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            ApplicationObject = GetService(typeof(DTE)) as DTE2;
            solutionEvents = ApplicationObject.Events.SolutionEvents;

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            ResxFinderMenuItem.Initialize(this);
            base.Initialize();
            MainWindowCommand.Initialize(this);
        }

        /// <summary>
        /// https://social.msdn.microsoft.com/Forums/vstudio/en-US/5f158826-2849-46ee-b669-1668d938a419/trying-to-load-systemwindowsinteractivity-for-a-vspackage-toolwindow?forum=vsx
        /// http://geekswithblogs.net/onlyutkarsh/archive/2013/06/02/loading-custom-assemblies-in-visual-studio-extensions-again.aspx
        /// https://stackoverflow.com/questions/52797/how-do-i-get-the-path-of-the-assembly-the-code-is-in
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("System.Windows.Interactivity"))
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                string assemblyDirectory = Path.GetDirectoryName(path);

                return Assembly.LoadFrom(Path.Combine(assemblyDirectory, "System.Windows.Interactivity.dll"));
            }
            return null;
        }

        #endregion
    }
}
