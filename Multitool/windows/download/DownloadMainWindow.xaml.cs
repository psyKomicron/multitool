using BusinessLayer.Network;
using BusinessLayer.Network.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiTool.DTO;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DownloadMainWindow : Window, INotifyPropertyChanged
    {
        private bool _showDownloadActivated;

        internal bool CurrentlyHyperLinked { get; set; }
        internal bool IsDownloading { get; set; }
        internal ObservableCollection<UrlHistoryViewModel> UrlHistory { get; set; }
        internal Downloader Downloader { get; set; }

        public List<string> DownloadHistory { get; private set; }
        public bool ShowDownloadActivated 
        {
            get => _showDownloadActivated;
            set
            {
                _showDownloadActivated = value;
                OnPropertyChanged();
            } 
        }

        public DownloadMainWindow()
        {
            InitializeComponent();
            InitializeWindow();
            DataContext = this;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        private void InitializeWindow()
        {
            IsDownloading = false;
            ShowDownloadActivated = false;
            UrlHistory = new ObservableCollection<UrlHistoryViewModel>();
            DownloadHistory = new List<string>(10);
            historyListView.ItemsSource = UrlHistory;
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

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

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
            if (e.Success)
            {
                UrlHistory.Add(new UrlHistoryViewModel() 
                { 
                    Date = DateTime.Now, 
                    Link = urlTextBox.Text,
                    TextColor = new SolidColorBrush(Colors.Black)
                });

                urlTextBox.Clear();
                downloadStatusLabel.Foreground = new SolidColorBrush(Colors.Black);
                downloadStatusLabel.Content = e.Message;
                DownloadHistory.Add(Downloader.DownloadedData);
                ShowDownloadActivated = true;
            }
            else
            {
                UrlHistory.Add(new UrlHistoryViewModel()
                {
                    Date = DateTime.Now,
                    Link = urlTextBox.Text,
                    TextColor = new SolidColorBrush(Colors.Red)
                });
                downloadStatusLabel.Foreground = new SolidColorBrush(Colors.Red);
                downloadStatusLabel.Content = "Download canceled/failed";
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
                urlTextBox.Foreground = new SolidColorBrush(Colors.Blue);
                urlTextBox.TextDecorations = TextDecorations.Underline;
                CurrentlyHyperLinked = true;
                e.Handled = true;
            }
            else if (CurrentlyHyperLinked)
            {
                urlTextBox.Foreground = new SolidColorBrush(Colors.Black);
                urlTextBox.TextDecorations = null;
                CurrentlyHyperLinked = false;
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
            DownloadHistory.Clear();
            DownloadHistory.Capacity = 10;
            ShowDownloadActivated = false;

            UrlHistory.Clear();
            e.Handled = true;

            downloadStatusLabel.Content = "History cleared";
        }

        #endregion
    }
}
