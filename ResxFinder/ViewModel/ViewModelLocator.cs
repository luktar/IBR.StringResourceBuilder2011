using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using NLog;
using ResxFinder.Core;
using ResxFinder.Core.SolutionAnalyzer;
using ResxFinder.Design;
using ResxFinder.Interfaces;
using System;
using System.Configuration;

namespace ResxFinder.ViewModel
{
    public class ViewModelLocator
    {
        private const string IS_TEST_MODE = "IsTestMode";

        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        private static ViewModelLocator instance;

        public static ViewModelLocator Instance
        {
            get
            {
                if (instance == null)
                    instance = new ViewModelLocator();
                return instance;
            }
        }

        private ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (IsTestMode())
            {

            }
            else
            {
                SimpleIoc.Default.Register<ISolutionHelper, SolutionHelper>();
                SimpleIoc.Default.Register<ISolutionProjectsHelper, SolutionProjectsHelper>();
                SimpleIoc.Default.Register<IParserManager, ParserManager>();
            }

            SimpleIoc.Default.Register<MainWindowControlViewModel>();
        }

        /// <summary>
        /// Value should exists in devenv.exe.config
        /// </summary>
        /// <returns></returns>
        private bool IsTestMode()
        {
            try
            {
                string isTestModeValue = ConfigurationManager.AppSettings[IS_TEST_MODE];

                if (string.IsNullOrEmpty(isTestModeValue))
                {
                    logger.Info($"{IS_TEST_MODE}: {false.ToString()}");
                    return false;
                }

                bool result = bool.Parse(isTestModeValue);
                logger.Info($"{IS_TEST_MODE}: {result.ToString()}");

                return result;
            } catch(Exception e)
            {
                logger.Warn(e, "Problem with reading configuration value: " + IS_TEST_MODE);
                return false;
            }
        }

        public T GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }
    }
}