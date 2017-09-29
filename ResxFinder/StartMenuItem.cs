using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;

namespace ResxFinder
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class StartMenuItem
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("577c37c5-ce0c-4dcd-9ce7-347a798e1ce8");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartMenuItem"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private StartMenuItem(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static StartMenuItem Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new StartMenuItem(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            AnalyzeSolution(dte);
        }

        private void AnalyzeSolution(DTE2 dte)
        {
            Solution solution = null;
            try
            {
                solution = dte.Solution;

                List<string> names = new List<string>();

                foreach (Project project in solution.Projects)
                {


                    if (project.Kind == "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}")
                        Console.WriteLine("Solution folder: " + project.Name);
                    else
                    {
                        foreach (ProjectItem projectItem in project.ProjectItems)
                        {

                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while analyzing solution." + Environment.NewLine + e);
            }
        }
    }
}
