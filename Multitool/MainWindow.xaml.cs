using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Window> openWindows = new List<Window>(3);
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Open<WindowType>() where WindowType : Window, new()
        {
            if (openWindows.Count > 0)
            {
                Window window = openWindows.Find((w) => w is WindowType);
                if (window != null)
                {
                    window.Activate();
                }
                else
                {
                    CreateAndOpen<WindowType>();
                }
            }
            else
            {
                CreateAndOpen<WindowType>();
            }
        }

        private void CreateAndOpen<WindowType>() where WindowType : Window, new()
        {
            Window w = new WindowType();
            w.Closed += ChildWindow_Closed;
            w.Show();
            openWindows.Add(w);
        }

        private void ChildWindow_Closed(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                Window instanceWindow = openWindows.Find((w) => w.Uid == window.Uid);
                if (instanceWindow != null)
                {
                    openWindows.Remove(instanceWindow);
                }
            }
        }

        private void OpenDownload_Click(object sender, RoutedEventArgs e)
        {
            Open<DownloadMainWindow>();
        }

        private void OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            Open<ExplorerWindow>();
        }

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            Open<PowerWindow>();
        }

        private void OpenSoon_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
