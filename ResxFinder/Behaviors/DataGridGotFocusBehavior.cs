using NLog;
using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ResxFinder.Behaviors
{
    /// <summary>
    /// https://stackoverflow.com/questions/42889566/how-to-perform-single-click-checkbox-selection-in-wpf-datagrid-with-ieditableobj
    /// </summary>
    public class DataGridGotFocusBehavior : Behavior<DataGrid>
    {
        private static Logger logger = NLogManager.Instance.GetCurrentClassLogger();

        DataGrid DataGrid
        {
            get { return AssociatedObject; }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            DataGrid.GotFocus += GotFocus;
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                DataGridCell cell = e.OriginalSource as DataGridCell;
                if (cell != null && cell.Column is DataGridCheckBoxColumn)
                {
                    DataGrid.BeginEdit();
                    CheckBox chkBox = cell.Content as CheckBox;
                    if (chkBox != null)
                    {
                        chkBox.IsChecked = !chkBox.IsChecked;
                    }
                }
            } catch(Exception ex)
            {
                logger.Warn(ex, "Problem with GotFocus event on DataGrid.");
            }
        }

        protected override void OnDetaching()
        {
            DataGrid.GotFocus -= GotFocus;
            base.OnDetaching();
        }
    }
}
