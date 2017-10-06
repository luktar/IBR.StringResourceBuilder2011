using System;
using System.Windows;
using System.Windows.Threading;
using EnvDTE;

namespace ResxFinder.Model
{
  public static class ExtensionMethods
  {

        private static Action EmptyDelegate = delegate() { };

    public static void Refresh(this UIElement uiElement)
    {
      uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
    }


    public static bool HasStartPoint(this CodeElement element)
    {
      try
      {
        //test whether access to the StartPoint throws an exception
        int line = element.StartPoint.Line;

        return (true);
      }
      catch (Exception /*ex*/)
      {
        return (false);
      }
    }


    }
} 
