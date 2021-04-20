using MultiToolBusinessLayer.Network.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MultiToolBusinessLayer.Network
{
    /// <summary>
    /// Used to connect to a url and execute a GET request on it.
    /// </summary>
    public class Downloader
    {
        private StreamReader _reader;

        public event EventHandler<DownloadEventArgs> IsDownloading;
        public event EventHandler<DownloadEventArgs> EndedDownload;

        public string DownloadedData { get; private set; }
        public string Url { get; private set; }

        public Downloader()
        {
            DownloadedData = string.Empty;
        }

        /// <summary>
        /// Asynchronously downloads data from a the <paramref name="url"/> url.
        /// </summary>
        /// <param name="url">url from wich to download</param>
        /// <returns></returns>
        public async Task Download(string url)
        {
            IsDownloading?.Invoke(this, new DownloadEventArgs(url, false, false));
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
                Url = url;
                EndedDownload?.Invoke(this, new DownloadEventArgs("Finished downloading : " + url, false, false));
            }
            catch (UriFormatException e)
            {
                EndedDownload?.Invoke(this, new DownloadEventArgs(e.Message, true, false));
            }
        }

        /// <summary>
        /// <para>Download a list of urls (<paramref name="urls"/>) using the <see cref="Download(string)"/> method.</para>
        /// <para>Discards the <see cref="Task"/> object return by <see cref="Download(string)"/> method.</para>
        /// </summary>
        /// <param name="urls"></param>
        public void Download(List<string> urls)
        {
            for (int i = 0; i < urls.Count; i++)
            {
                _ = Download(urls[i]);
            }
        }

        /// <summary>
        /// Cancels a download by closing the <see cref="StreamReader"/> created by <see cref="Download(string)"/> 
        /// </summary>
        public void Cancel()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader.Dispose();
                EndedDownload?.Invoke(this, new DownloadEventArgs("User cancelled download", false, true));
            }
        }
    }
}
