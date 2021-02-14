using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MultiTool.ViewModels
{
    public class DownloadData : DefaultWindowData
    {
        public ObservableCollection<UrlHistoryViewModel> History { get; set; }

        public DownloadData()
        {
            History = new ObservableCollection<UrlHistoryViewModel>();
            Height = 600;
            Width = 900;
        }
    }
}
