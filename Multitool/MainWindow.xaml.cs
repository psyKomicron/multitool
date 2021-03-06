﻿using BusinessLayer.Controllers;
using MultiTool.DTO;
using MultiTool.Tools;
using MultiTool.Windows;
using System;
using System.Reflection;
using System.Windows;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, ISerializableWindow
    {
        private readonly string gitHub = "https://github.com/psyKomicron/multitool/blob/main/README.md";

        public string AppVersion { get; set; }

        public MainWindowDTO Data { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public void Serialize()
        {
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
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

        private void OpenSoon_Click(object sender, RoutedEventArgs e) 
        {
            new DefaultBrowserController()
            {
                Uri = new Uri(gitHub)
            }.Execute();
        }

        #endregion

    }
}
