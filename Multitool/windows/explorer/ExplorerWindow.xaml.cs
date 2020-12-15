using MultiTool.DTO;
using MultiTool.Tools;
using MultiTool.Windows;
using System;
using System.Collections.Generic;
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

        public ExplorerWindow()
        {
            InitializeComponent();
        }

        public void Deserialize()
        {
            WindowManager.GetPreferenceManager().GetWindowManager<ExplorerWindowDTO>(Name);
        }

        public void Serialize()
        {
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
        }
    }
}
