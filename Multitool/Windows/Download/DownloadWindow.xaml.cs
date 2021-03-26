using BusinessLayer.Network;
using BusinessLayer.Network.Events;
using Microsoft.Win32;
using MultiTool.Tools;
using MultiTool.ViewModels;
using MultiTool.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DownloadWindow : Window, INotifyPropertyChanged, ISerializableWindow
    {
        //private readonly Regex isExtension = new Regex(@"([a-z])+");
        private bool _showDownloadActivated;

        public bool CurrentlyHyperLinked { get; set; }

        public bool IsDownloading { get; set; }

        public Downloader Downloader { get; set; }

        public DownloadData Data { get; set; }

        public bool ShowDownloadActivated 
        {
            get => _showDownloadActivated;
            set
            {
                _showDownloadActivated = value;
                Tool.FireEvent(PropertyChanged, this);
            } 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public DownloadWindow()
        {
            InitializeComponent();
            InitializeWindow();
            DataContext = this;
        }

        #region methods

        public void Serialize()
        {
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
        }

        public void Deserialize()
        {
            Data = WindowManager.GetPreferenceManager().GetWindowManager<DownloadData>(Name);
        }

        private void InitializeWindow()
        {
            IsDownloading = ShowDownloadActivated = false;            
        }

        private bool IsUrl(string url)
        {
            return Regex.Match(url, @"(https:\/\/www.youtube.com\/watch\?v=[A-z0-9]{1,})", RegexOptions.IgnoreCase).Success;
        }

        private async Task DownloadUrl(string url)
        {
            Downloader = new Downloader();
            Downloader.IsDownloading += OnDownload;
            Downloader.EndedDownload += OnDownloadEnd;

            await Downloader.Download(url);
        }

        private void SaveDownload(string url)
        {
            string filter = "*";
            try
            {
                filter = GetExtensions(url);
            }
            catch (ArgumentException ae)
            {
                Console.Error.WriteLine("Could not get extension from url/uri " + url.Substring(url.Length - 10));
                Console.Error.WriteLine(ae.ToString());
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = "Save download from multi-tool",
                FileName = DateTime.Now.ToString().Replace('/', '-').Replace(':', '-'),
                Filter = GetExtensions(url)
            };

            bool ok = saveFileDialog.ShowDialog() ?? false;
            if (ok && !string.IsNullOrEmpty(saveFileDialog.FileName))
            {
                using (FileStream fs = (FileStream)saveFileDialog.OpenFile())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Downloader.DownloadedData.ToCharArray());
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private string GetExtensions(string url)
        {
            return Path.GetExtension(url);
        }

        #endregion

        #region events handlers

        private void OnDownload(object sender, DownloadEventArgs e)
        {
            IsDownloading = true;
            cancelButton.IsEnabled = true;
            downloadButton.IsEnabled = false;
        }

        private void OnDownloadEnd(object sender, DownloadEventArgs e)
        {
            IsDownloading = false;
            cancelButton.IsEnabled = false;
            downloadButton.IsEnabled = true;
            if (!e.Cancelled && !e.Crashed)
            {
                Data.History.Add(new UrlHistoryViewModel() 
                { 
                    Date = DateTime.Now, 
                    Link = urlTextBox.Text,
                });

                urlTextBox.Clear();
                downloadStatusLabel.Content = e.Message;
                ShowDownloadActivated = true;
            }
            else
            {
                Data.History.Add(new UrlHistoryViewModel()
                {
                    Date = DateTime.Now,
                    Link = urlTextBox.Text,
                });


                downloadStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                if (e.Cancelled)
                {
                    downloadStatusLabel.Content = "Download cancelled";
                }
                else if (e.Crashed)
                {
                    downloadStatusLabel.Content = "Download failed";
                }
                else
                {
                    downloadStatusLabel.Content = "Download cancelled/failed";
                }
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDownloading && (!string.IsNullOrEmpty(urlTextBox.Text) || !string.IsNullOrWhiteSpace(urlTextBox.Text)))
            {
                e.Handled = true;
                string text = urlTextBox.Text;

                try
                {
                    await DownloadUrl(text);
                } 
                catch (ObjectDisposedException) { } 
                catch (WebException) { }
            }
        }

        private void DownloadListButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsDownloading)
            {
                var urls = historyListView.SelectedItems;
                List<UrlHistoryViewModel> selectedItems = new List<UrlHistoryViewModel>();

                foreach (var url in urls)
                {
                    selectedItems.Add(url as UrlHistoryViewModel);
                }
                e.Handled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Downloader.Cancel();
            e.Handled = true;
        }

        private void HistoryListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count != 0)
            {
                downloadListButton.IsEnabled = true;
                e.Handled = true;
            }
        }

        private void UrlTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsUrl(urlTextBox.Text))
            {
                urlTextBox.TextDecorations = TextDecorations.Underline;
                CurrentlyHyperLinked = true;
            }
            else if (CurrentlyHyperLinked)
            {
                urlTextBox.TextDecorations = null;
                CurrentlyHyperLinked = false;
            }
            e.Handled = true;
        }

        private void SaveDownload_Click(object sender, RoutedEventArgs e)
        {
            if (Downloader != null && Downloader.DownloadedData != string.Empty)
            {
                SaveDownload(Downloader.Url);
                e.Handled = true;
            }
        }

        private void ShowDownload_Click(object sender, RoutedEventArgs e)
        {
            if (Downloader != null && Downloader.DownloadedData != string.Empty)
            {
                new DownloadView(Downloader).Show();
                e.Handled = true;
            }
        }

        private void ClearHistory_Click(object sender, RoutedEventArgs e)
        {
            ShowDownloadActivated = false;

            Data.History.Clear();
            e.Handled = true;

            downloadStatusLabel.Content = "History cleared";
        }

        #endregion

        private void MultiToolWindowChrome_MinimizeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void MultiToolWindowChrome_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MultiToolWindowChrome_CloseClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }
    }
}
