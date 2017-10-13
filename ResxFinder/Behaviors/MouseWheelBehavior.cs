using NLog;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace ResxFinder.Behaviors
{
    public class MouseWheelBehavior : Behavior<ScrollViewer>
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        ScrollViewer ScrollViewer
        {
            get { return AssociatedObject; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            ScrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
        }

        protected override void OnDetaching()
        {
            ScrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            base.OnDetaching();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
