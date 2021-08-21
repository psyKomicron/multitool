using Multitool.Reflection.ObjectFlatteners;

using System.Collections.ObjectModel;

namespace MultitoolWPF.ViewModels
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
