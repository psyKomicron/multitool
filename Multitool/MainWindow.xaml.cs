using BusinessLayer.PreferencesManagers;
using BusinessLayer.PreferencesManagers.Json;
using BusinessLayer.PreferencesManagers.Xml;
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
                .AddPreferenceManager(new XmlWindowPreferenceManager() 
                { 
                    ItemName = Name,
                    Properties = properties
                });
        }

        public void Deserialize()
        {
            Data = new MainWindowDTO();
            IWindowPreferenceManager manager = Tool.GetPreferenceManager().GetWindowManager(Name);

            if (manager != null)
            {
                Data = (Application.Current as App).PropertyLoader.Load<MainWindowDTO>(manager.Properties);
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
