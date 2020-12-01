using System.Collections.Generic;

namespace MultiTool.DTO
{
    public class DownloadDTO : DefaultWindowDTO
    {
        public List<UrlHistoryViewModel> History { get; set; }

        public DownloadDTO()
        {
            Height = 600;
            Width = 900;
            History = new List<UrlHistoryViewModel>();
        }
    }
}
