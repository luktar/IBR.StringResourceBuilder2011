namespace ResxFinder.Views
{
    using ResxFinder.ViewModel;
    using System.Windows.Controls;

    public partial class MainWindowControl : UserControl
    {
        public MainWindowControl()
        {
            this.InitializeComponent();
            DataContext = ViewModelLocator.Instance.GetInstance<MainWindowControlViewModel>();
        }
    }
}