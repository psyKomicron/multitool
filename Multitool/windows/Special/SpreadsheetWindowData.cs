using Multitool.Reflection.ObjectFlatteners;
using MultiTool.ViewModels;
using System.Collections.ObjectModel;

namespace MultiTool.Windows
{
    public class SpreadsheetWindowData : DefaultWindowData
    {
        [ListFlattener(nameof(Items), typeof(CommonXmlObjectFlattener))]
        public ObservableCollection<SpreadsheetVM> Items { get; set; }

        public SpreadsheetWindowData()
        {
            Items = new ObservableCollection<SpreadsheetVM>();
        }
    }
}
