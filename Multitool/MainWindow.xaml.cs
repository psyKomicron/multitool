using BusinessLayer.PreferencesManagers;
using MultiTool.DTO;
using MultiTool.Tools;
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

            WindowManager.GetPreferenceManager()
                .AddWindowManager(new WindowPreferenceManager() 
                { 
                    ItemName = Name,
                    Properties = properties
                });
        }

        public void Deserialize()
        {
            Data = WindowManager.GetPreferenceManager().GetWindowManager<MainWindowDTO>(Name);

            if (Data == null)
            {
                Data = new MainWindowDTO();
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

        private void OpenDownload_Click(object sender, RoutedEventArgs e) => WindowManager.Open<DownloadWindow>();

        private void OpenExplorer_Click(object sender, RoutedEventArgs e) => WindowManager.Open<ExplorerWindow>();

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e) => WindowManager.Open<PowerWindow>();

        private void OpenSoon_Click(object sender, RoutedEventArgs e) { }

        #endregion

    }
}
