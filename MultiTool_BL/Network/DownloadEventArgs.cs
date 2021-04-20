using System;

namespace MultiToolBusinessLayer.Network.Events
{
    public class DownloadEventArgs : EventArgs
    {
        public string Message { get; }
        public bool Crashed { get; }
        public bool Cancelled { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">Url</param>
        /// <param name="crashed">Download crashed</param>
        /// <param name="cancelled">Download was cancelled</param>
        public DownloadEventArgs(string url, bool crashed, bool cancelled)
        {
            Message = url;
            Crashed = crashed;
            Cancelled = cancelled;
        }

        public DownloadEventArgs()
        {
            Message = default;
            Crashed = default;
            Cancelled = default;
        }
    }
}
