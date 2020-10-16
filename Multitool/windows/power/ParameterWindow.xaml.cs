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

namespace MultiTool.windows.power
{
    /// <summary>
    /// Interaction logic for ParameterWindow.xaml
    /// </summary>
    public partial class ParameterWindow : Window
    {
        public ObservableCollection<TreeViewItem> Options { get; set; }

        public ParameterWindow()
        {
            InitializeComponent();
            Options = new ObservableCollection<TreeViewItem>();
            TreeViewItem item = new TreeViewItem();
            item.Header = "Options";
            Options.Add(item);
        }
    }
}
