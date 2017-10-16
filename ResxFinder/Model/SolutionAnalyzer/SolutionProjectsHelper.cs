using EnvDTE;
using NLog;
using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Model.SolutionAnalyzer
{
    public class SolutionProjectsHelper : ISolutionProjectsHelper
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        /// <summary>
        /// http://www.wwwlicious.com/2011/03/29/envdte-getting-all-projects-html/
        /// </summary>
        public const string PROJECT_KIND_SOLUTION_FOLDER = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

        private Solution Solution { get; set; }

        public SolutionProjectsHelper(ISolutionHelper solutionHelper)
        {
            Solution = solutionHelper.GetSolution();
        }

        public List<Project> GetProjects(ISettings settings)
        {
            string currentProjectName = string.Empty;
            try
            {
                Projects projects = Solution.Projects;
                List<Project> list = new List<Project>();

                var item = projects.GetEnumerator();

                while (item.MoveNext())
                {
                    var project = item.Current as Project;               

                    if (project == null)
                    {
                        continue;
                    }

                    currentProjectName = project.Name;

                    if(Contains(currentProjectName, settings.IgnoredProjects))
                    {                      
                        logger.Debug(Environment.NewLine + "Ignore project: " + currentProjectName + Environment.NewLine);
                        continue;
                    }

                    logger.Debug(Environment.NewLine + "Analyzing project: " + currentProjectName + Environment.NewLine);

                    if (project.Kind == PROJECT_KIND_SOLUTION_FOLDER)
                    {
                        list.AddRange(GetSolutionFolderProjects(project));
                    }
                    else
                    {
                        list.Add(project);
                    }
                }

                return list;
            } catch
            {
                logger.Error("Unable to obtain projects from solution.");
                throw;
            }
        }

        private bool Contains(string name, List<string> ignoreStrings)
        {
            foreach(string element in ignoreStrings)
            {
                if (name.Contains(element)) return true;
            }
            return false;
        }

        private List<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                {
                    continue;
                }

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == PROJECT_KIND_SOLUTION_FOLDER)
                {
                    list.AddRange(GetSolutionFolderProjects(subProject));
                }
                else
                {
                    list.Add(subProject);
                }
            }
            return list;
        }

    }
}
