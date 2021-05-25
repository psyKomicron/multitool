using Multitool.Controllers;
using MultiTool.ViewModels;
using MultiTool.Tools;
using MultiTool.Windows;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using Multitool.Monitoring;
using System.Timers;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISerializableWindow
    {
        private const string gitHub = "https://github.com/psyKomicron/multitool/blob/main/README.md";
        private CpuMonitor cpuMonitor = new CpuMonitor();
        private Timer cpuTimer = new Timer(500);

        public string AppVersion { get; set; }

        public MainWindowData Data { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowManager(Data, Name);
        }

        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowManager<MainWindowData>(Name);

            if (Data == null)
            {
                Data = new MainWindowData();
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                UpdateLayout();
            }
        }

        private void InitializeWindow()
        {
            Deserialize();
            DataContext = this;

            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            cpuTimer.Elapsed += CpuTimer_Elapsed;
            cpuTimer.Start();
        }

        #region events

        private void CpuTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!cpuMonitor.Ready)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage.Text = cpuMonitor.GetCpuUsage().ToString();
            });
        }

        private void MultiToolMainWindow_Closed(object sender, EventArgs e)
        {
            cpuTimer.Stop();
            cpuTimer.Dispose();
            cpuMonitor.Dispose();
            Serialize();
        }

        private void OpenDownload_Click(object sender, RoutedEventArgs e) => WindowManager.Open<SpreadsheetWindow>();

        private void OpenExplorer_Click(object sender, RoutedEventArgs e) => WindowManager.Open<ExplorerWindow>();

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e) => WindowManager.Open<PowerWindow>();

        private void OpenSoon_Click(object sender, RoutedEventArgs e) 
        {
            new DefaultBrowserController()
            {
                Uri = new Uri(gitHub)
            }.Execute();
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                DragMove();
            }
        }

        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void WindowMinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            WindowState = WindowState.Minimized;
        }

        #endregion
    }
}
