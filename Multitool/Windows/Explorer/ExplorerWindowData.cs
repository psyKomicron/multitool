using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.ViewModels
{
    public class ExplorerWindowData : DefaultWindowData
    {
        public string LastUsedPath { get; set; }
        public ObservableCollection<string> History { get; set; }

        public ExplorerWindowData()
        {
            LastUsedPath = string.Empty;
            History = new ObservableCollection<string>();
        }
    }
}
