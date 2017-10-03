/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:MvvmLight1.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using Microsoft.Practices.ServiceLocation;
using ResxFinder.Core;
using ResxFinder.Design;
using ResxFinder.Interfaces;

namespace ResxFinder.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
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

            if (ViewModelBase.IsInDesignModeStatic)
            {
                //SimpleIoc.Default.Register<IParser, DesignParser>();
            }
            else
            {
                //SimpleIoc.Default.Register<IParser, Parser>();
            }

            SimpleIoc.Default.Register<MainWindowControlViewModel>();
        }

        public T GetViewModel<T>()
        {
            return ServiceLocator.Current.GetInstance<T>();
        }
    }
}