using System.Collections.ObjectModel;
using Multitool.Reflection.ObjectFlatteners;

namespace MultiTool.ViewModels
{
    public class ExplorerWindowData : DefaultWindowData
    {
        public string LastUsedPath { get; set; }
        [ListFlattener(nameof(History), typeof(PrimitiveXmlFlattener))]
        public ObservableCollection<string> History { get; set; }
        public double TTL { get; set; }

        public ExplorerWindowData()
        {
            LastUsedPath = string.Empty;
            History = new ObservableCollection<string>();
        }
    }
}
