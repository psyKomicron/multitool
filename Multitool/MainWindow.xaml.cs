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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void OpenDownload_Click(object sender, RoutedEventArgs e)
        {
            new DownloadMainWindow().Show();
        }

        private void OpenExplorer_Click(object sender, RoutedEventArgs e)
        {
            new ExplorerWindow().Show();
        }

        private void OpenPowerSettings_Click(object sender, RoutedEventArgs e)
        {
            new PowerWindow().Show();
        }

        private void OpenSoon_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
