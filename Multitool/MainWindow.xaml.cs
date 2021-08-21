using Multitool.Controllers;
using Multitool.PerformanceMonitors;

using MultitoolWPF.Tools;
using MultitoolWPF.UserControls;
using MultitoolWPF.ViewModels;
using MultitoolWPF.Windows;
using MultitoolWPF.Windows.ControlPanels;

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
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
        private System.Timers.Timer perfMonTimer = new System.Timers.Timer(250);
        private CancellationTokenSource instanceTokenSource;

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

            instanceTokenSource =  CancellationTokenSource.CreateLinkedTokenSource(localCpuMonitor.CancellationTokenSource.Token, localRamMonitor.CancellationTokenSource.Token);

            await Task.WhenAll(localCpuMonitor.InstanciationTask, localRamMonitor.InstanciationTask);

            if (instanceTokenSource.IsCancellationRequested)
            {
                return;
            }

            cpuMonitor = localCpuMonitor;
            ramMonitor = localRamMonitor;
            perfMonTimer.Elapsed += (object sender, ElapsedEventArgs e) => Application.Current.Dispatcher.Invoke(UpdatePerfsStats);
            perfMonTimer.Start();

            instanceTokenSource.Dispose();
            instanceTokenSource = null;
        }

        private void AddAndSwitch(string headerName, FrameworkElement content)
        {
            TabItem tab = new TabItem()
            {
                Foreground = new SolidColorBrush(Colors.White),
                Content = content
            };
            ClosableTabHeader header = new ClosableTabHeader(tab)
            {
                Title = headerName
            };
            tab.Header = header;
            header.Close += Header_Close;
            _ = Controls_Panel.Items.Add(tab);
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (instanceTokenSource != null)
            {
                instanceTokenSource.Cancel();
            }

            perfMonTimer.Stop();
            perfMonTimer.Dispose();
            if (cpuMonitor != null)
            {
                cpuMonitor.Dispose();
                ramMonitor.Dispose();
            }
            Serialize();
        }
        #endregion

        #region tools menu
        private void PowerCapabilitiesPanelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAndSwitch("Power capabilities", new PowerCapabilitiesPanel()
            {
                Margin = new Thickness(5)
            });
        }

        private void PowerPanelButton_Click(object sender, RoutedEventArgs e)
        {
            AddAndSwitch("Power panel", new PowerPlansPanel());
        }

        private void Header_Close(object sender, RoutedEventArgs e)
        {
            TabItem tab = (sender as ClosableTabHeader).TabItem;
            Controls_Panel.Items.Remove(tab);
        }

        private void CopySpotlightWallpapers_Click(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = new TextBlock()
            {
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14
            };
            AddAndSwitch("Copying Spotlight files", textBlock);

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
                try
                {
                    System.Drawing.Image image = new Bitmap(files[i]);
                    if (image.Height >= 1080 && image.Width >= 1920)
                    {
                        fileInfo = new FileInfo(files[i]);
                        fileInfo = fileInfo.Extension != ".png"
                            ? fileInfo.CopyTo(myPicturesPath + "\\" + fileInfo.Name + ".png", true)
                            : fileInfo.CopyTo(myPicturesPath + "\\" + fileInfo.Name, true);
                        textBlock.Inlines.Add("Successfully moved and renamed -> " + fileInfo.Name + "\n");
                        Trace.WriteLine("Successfully moved and renamed : " + fileInfo.Name);
                    }
                    else
                    {
                        Trace.WriteLine("Not moving " + files[i] + " (too small)");
                    }
                }
#if TRACE
                catch (ArgumentException ae)
                {
                    Console.WriteLine(ae.ToString());
                    textBlock.Inlines.Add("Failed to move -> " + files[i] + "\n");
                }
#else
                catch (ArgumentException) { }
#endif
            }
            //WindowManager.Open<ExplorerWindow>(myPicturesPath);
        }
        #endregion

        #endregion
    }
}
