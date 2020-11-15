using BusinessLayer.PreferencesManager;
using MultiTool.DTO;
using MultiTool.Windows;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISerializableWindow
    {
        public string AppVersion { get; set; }

        public MainWindowDTO Data { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public void Serialize()
        {
            Dictionary<string, string> properties = Tool.Flatten(Data);

            Tool.GetPreferenceManager()
                .AddPreferenceManager(new JsonWindowPreferenceManager() 
                { 
                    ItemName = Name,
                    Values = properties 
                });
        }

        public void Deserialize()
        {
            Data = new MainWindowDTO();
            IWindowPreferenceManager manager = Tool.GetPreferenceManager().GetWindowManager(Name);
            if (manager != null)
            {
                Data.Height = manager.Values["Height"] == null ? Data.Height : double.Parse(manager.Values["Height"]);
                Data.Width  = manager.Values["Width"] == null ? Data.Width : double.Parse(manager.Values["Width"]);
                Data.Left   = manager.Values["Left"] == null ? Data.Left : double.Parse(manager.Values["Left"]);
                Data.Top    = manager.Values["Top"] == null ? Data.Top : double.Parse(manager.Values["Top"]);
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                UpdateLayout();
            }
        }

        private void InitializeWindow()
        {
            Deserialize();
            DataContext = this;

            AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        #region events

        private void MultiToolMainWindow_Closed(object sender, EventArgs e) => Serialize();

        private void OpenDownload_Click(object sender, RoutedEventArgs e) => WindowManager.Open<DownloadMainWindow>();

        private void OpenExplorer_Click(object sender, RoutedEventArgs e) => WindowManager.Open<ExplorerWindow>();

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e) => WindowManager.Open<PowerWindow>();

        private void OpenSoon_Click(object sender, RoutedEventArgs e) { }

        #endregion

    }
}
