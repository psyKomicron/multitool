using Multitool.Controllers;
using Multitool.NTInterop;
using Multitool.PerformanceMonitors;

using MultitoolWPF.Tools;
using MultitoolWPF.UserControls;
using MultitoolWPF.ViewModels;
using MultitoolWPF.Windows;
using MultitoolWPF.Windows.ControlPanels;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultitoolWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISerializableWindow
    {
        private const string gitHub = "https://github.com/psyKomicron/multitool/blob/main/README.md";
        private IPerformanceMonitor cpuMonitor;
        private IPerformanceMonitor ramMonitor;
        private Timer cpuTimer = new Timer(250);

        public string AppVersion { get; set; }

        public MainWindowData Data { get; set; }

        public MainWindow()
        {
            WindowManager.MainWindow = this;
            InitializeComponent();
            InitializeWindow();
        }

        #region serialise/deserialise
        public void Serialize()
        {
            Data.LastSelectedIndex = Window_TabControl.SelectedIndex;
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
            if (!string.IsNullOrWhiteSpace(Data.StartWindow))
            {
                Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].FullName == Data.StartWindow)
                    {
                        if (!WindowManager.Open(types[i]))
                        {
                            Console.WriteLine("Failed to open the start-up window (type : " + types[i].Name + ")");
                        }
                    }
                }
            }
        }
        #endregion

        #region private

        private async void InitializeWindow()
        {
            Deserialize();
            DataContext = this;

            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            AsyncPerformanceMonitor localCpuMonitor = new AsyncPerformanceMonitor(MonitorCategory.ProcessorUsageTotal);
            AsyncPerformanceMonitor localRamMonitor = new AsyncPerformanceMonitor(MonitorCategory.MemoryGlobal);

            await Task.WhenAll(localCpuMonitor.InstanciationTask, localRamMonitor.InstanciationTask);

            cpuMonitor = localCpuMonitor;
            ramMonitor = localRamMonitor;
            cpuTimer.Elapsed += (object sender, ElapsedEventArgs e) => Application.Current.Dispatcher.Invoke(UpdatePerfsStats);
            cpuTimer.Start();
        }

        private void AddAndSwitch(FrameworkElement element)
        {
            Controls_Panel.Items.Add(element);
            Controls_Panel.SelectedIndex = Controls_Panel.Items.Count - 1;
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
        private void OpenDownload_Click(object sender, RoutedEventArgs e)
        {
            _ = WindowManager.Open<SpreadsheetWindow>();
            Data.StartWindow = typeof(SpreadsheetWindow).FullName;
        }

        private void OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            _ = WindowManager.Open<ExplorerWindow>();
            Data.StartWindow = typeof(ExplorerWindow).FullName;
        }

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            _ = WindowManager.Open<PowerWindow>();
            Data.StartWindow = typeof(PowerWindow).FullName;
        }

        private void OpenSoon_Click(object sender, RoutedEventArgs e)
        {
            new DefaultBrowserController()
            {
                Uri = new Uri(gitHub)
            }.Execute();
        }

        private void PreferencesButton_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Multitool\preferences\userpreferences.xml");
        }
        #endregion

        #region window
        private void UpdatePerfsStats()
        {
            CpuUsage.Text = cpuMonitor.GetStats().ToString("F2");
            float ram = ramMonitor.GetStats() / 1_048_576; // Mb
            RamUsage_TextBlock.Text = ram.ToString("F2");
        }

        private void MultiToolMainWindow_Closed(object sender, EventArgs e)
        {
            cpuTimer.Stop();
            cpuTimer.Dispose();
            cpuMonitor.Dispose();
            Serialize();
        }
        #endregion

        #region tools menu
        private void PowerCapabilitiesPanelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAndSwitch(new TabItem()
            {
                Header = "Power capabilities",
                Foreground = new SolidColorBrush(Colors.White),
                Content = new PowerCapabilitiesPanelControl()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(5)
                }
            });
        }

        private void PowerPanelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAndSwitch(new TabItem()
            {
                Header = "Power panel",
                Foreground = new SolidColorBrush(Colors.White),
                Content = new PowerPlansPanel()
            });
        }

        private void CopySpotlightWallpapers_Click(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 10
            };
            AddAndSwitch(new TabItem()
            {
                Header = "Copying Spotlight files",
                Foreground = new SolidColorBrush(Colors.White),
                Content = textBlock
            });

            string localappdata = Environment.GetEnvironmentVariable("LOCALAPPDATA");
            if (string.IsNullOrEmpty(localappdata))
            {
                throw new Exception("LOCALAPPDATA env variable is empty");
            }
            string spotlight = localappdata + Tool.GetStringResource("SpotlightWallpapers");
            string myPicturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Spotlight";

            if (!Directory.Exists(myPicturesPath))
            {
                _ = Directory.CreateDirectory(myPicturesPath);
            }

            string[] files = Directory.GetFiles(spotlight);
            FileInfo fileInfo;
            for (int i = 0; i < files.Length; i++)
            {
                fileInfo = new FileInfo(files[i]);
                fileInfo = fileInfo.Extension != ".png"
                    ? fileInfo.CopyTo(myPicturesPath + "\\" + fileInfo.Name + ".png", true)
                    : fileInfo.CopyTo(myPicturesPath + "\\" + fileInfo.Name, true);
                Console.WriteLine("Successfully moved and renamed -> " + fileInfo.Name);
                textBlock.Inlines.Add("Successfully moved and renamed -> " + fileInfo.Name + "\n");
            }
            //WindowManager.Open<ExplorerWindow>(myPicturesPath);
        }
        #endregion

        #endregion
    }
}
