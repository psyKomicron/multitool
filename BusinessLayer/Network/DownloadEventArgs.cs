using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Network.Events
{
    public class DownloadEventArgs : EventArgs
    {
        public string Message { get; }
        public bool Success { get; }

        public DownloadEventArgs(string url, bool success = false)
        {
            Message = url;
            Success = success;
        }

        public DownloadEventArgs()
        {
            Message = default;
            Success = default;
        }
    }
}
