using BusinessLayer.Network.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Network
{
    public class Downloader
    {
        private FileInfo fileInfo;
        private StreamReader _reader;

        public event EventHandler<DownloadEventArgs> IsDownloading;
        public event EventHandler<DownloadEventArgs> EndedDownload;

        public string DownloadedData { get; private set; }

        public Downloader()
        {
            fileInfo = new FileInfo(Path.GetTempFileName());
            DownloadedData = string.Empty;
        }

        public async Task Download(string url)
        {
            IsDownloading?.Invoke(this, new DownloadEventArgs(url));
            try
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    _reader = new StreamReader(dataStream);
                    DownloadedData = await _reader.ReadToEndAsync();
                }
                response.Close();
                EndedDownload?.Invoke(this, new DownloadEventArgs("Finished downloading : " + url, true));
            }
            catch (UriFormatException e)
            {
                EndedDownload?.Invoke(this, new DownloadEventArgs(e.Message, false));
            }
        }

        public void Download(List<string> urls)
        {
            for (int i = 0; i < urls.Count; i++)
            {
                _ = Download(urls[i]);
            }
        }

        public void Cancel()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                EndedDownload?.Invoke(this, new DownloadEventArgs("User cancelled download", false));
            }
        }
    }
}
