using Multitool.Controllers;
using Multitool.Monitoring;

using MultitoolWPF.Tools;
using MultitoolWPF.ViewModels;
using MultitoolWPF.Windows;

using System;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultitoolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISerializableWindow
    {
        private const string gitHub = "https://github.com/psyKomicron/multitool/blob/main/README.md";
        private CpuMonitor cpuMonitor = new CpuMonitor();
        private Timer cpuTimer = new Timer(250);

        public string AppVersion { get; set; }

        public MainWindowData Data { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        #region serialise/deserialise
        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }

        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowData<MainWindowData>(Name);

            if (Data == null)
            {
                Data = new MainWindowData();
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                UpdateLayout();
            }
        }
        #endregion

        #region private

        private void InitializeWindow()
        {
            Deserialize();
            DataContext = this;

            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            cpuTimer.Elapsed += CpuTimer_Elapsed;
            cpuTimer.Start();
        }

        #endregion private

        #region events

        #region chrome
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

        #region home menu
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

        private void PowerCapabilitiesPanelButton_Click(object sender, RoutedEventArgs e)
        {
            Controls_Panel.Items.Add(new TabItem()
            {
                Header = "Power capabilities",
                Foreground = new SolidColorBrush(Colors.White),
                Content = new PowerPanelControl()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(5)
                }
            });
        }

        private void PowerPanelButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void CpuTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!cpuMonitor.Ready)
            {
                return;
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                CpuUsage.Text = cpuMonitor.GetCpuUsage().ToString("F2");
            });
        }

        private void MultiToolMainWindow_Closed(object sender, EventArgs e)
        {
            cpuTimer.Stop();
            cpuTimer.Dispose();
            cpuMonitor.Dispose();
            Serialize();
        }

        private void Window_TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Window_TabControl.SelectedIndex == 1)
            {
                // draw a dick 8=========0 (>avg)
            }
        }
        #endregion
    }
}
