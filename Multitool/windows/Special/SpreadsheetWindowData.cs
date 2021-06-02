using Multitool.Reflection.ObjectFlatteners;
using Multitool.ViewModels;
using System.Collections.ObjectModel;

namespace Multitool.Windows
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
