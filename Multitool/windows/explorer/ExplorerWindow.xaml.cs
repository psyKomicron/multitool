using MultiTool.DTO;
using MultiTool.Tools;
using MultiTool.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    /// Interaction logic for ExplorerWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window, ISerializableWindow
    {
        public ExplorerWindowDTO Data { get; set; }

        public ObservableCollection<string> FolderHistory { get; private set; }

        public ExplorerWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            DataContext = this;

            FolderHistory = new ObservableCollection<string>
            {
                "Folder 1",
                "Folder 2",
                "Long Long Long name folder 3"
            };
            FolderHistory.Add("new");
        }

        public void Deserialize()
        {
            Data = WindowManager.GetPreferenceManager().GetWindowManager<ExplorerWindowDTO>(Name);
        }

        public void Serialize()
        {
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
        }

        private void FolderHistory_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
