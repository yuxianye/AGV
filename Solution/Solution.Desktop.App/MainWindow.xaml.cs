using MahApps.Metro.Controls;
using System.Windows;

namespace Solution.Desktop.App
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            SetTitle();
        }

        public void SetTitle()
        {
            //Icon = "pack://application:,,,/Solution.Desktop.Resource;component/Images/logo_64X64.ico"
            Title = Application.Current.TryFindResource(@"AppName")
                     + @" "
                     + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            //Application.Current.MainWindow.Title = Utility.Windows.ResourceHelper.LoadString(@"AppName")
            //    + " "
            //    + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
            //    + "."
            //    + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor;

            //Application.Current.MainWindow.Title = Utility.Windows.ResourceHelper.LoadString("AppName")
            //+ " "
            //+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major
            //+ "."
            //+ System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor;
        }

        private void MetroWindow_Closed(object sender, System.EventArgs e)
        {
            mainView.Dispose();
            Application.Current.Resources.Clear();
            Application.Current.Resources = null;
        }
    }
}
