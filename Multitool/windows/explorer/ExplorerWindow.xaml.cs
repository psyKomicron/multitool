using MultiTool.Tools;
using MultiTool.ViewModels;
using MultiToolBusinessLayer;
using MultiToolBusinessLayer.FileSystem;
using MultiToolBusinessLayer.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiTool.Windows
{
    /// <summary>
    /// Interaction logic for ExplorerWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window, ISerializableWindow, INotifyPropertyChanged
    {
        private string _currentPath;
        private UriCleaner cleaner = new UriCleaner();
        private Stack<string> pathHistory = new Stack<string>(10);
        private Stack<string> nextPathStack = new Stack<string>(10);
        private FileSystemManager fileSystemManager;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Stopwatch eventStopwatch = new Stopwatch();

        public event PropertyChangedEventHandler PropertyChanged;

        public ExplorerWindowData Data { get; set; }
        public ObservableCollection<PathItemVM> CurrentFiles { get; private set; }
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                _currentPath = value;
                NotifyPropertyChanged();
            }
        }

        public ExplorerWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public void Deserialize()
        {
            Data = WindowManager.GetPreferenceManager().GetWindowManager<ExplorerWindowData>(Name);

            if (Data.TTL != default)
            {
                fileSystemManager = FileSystemManager.Get(Data.TTL, true);
            }
            else
            {
                fileSystemManager = FileSystemManager.Get();
            }

            fileSystemManager.Progress += FileSystemManager_Progress;
            fileSystemManager.Exception += FileSystemManager_Exception;
        }

        public void Serialize()
        {
            Data.LastUsedPath = CurrentPath;
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
        }

        private void InitializeWindow()
        {
            DataContext = this;
            CurrentFiles = new ObservableCollection<PathItemVM>();
        }

        private async Task DisplayFiles(string path)
        {
            #region clear displays
            CurrentFiles.Clear();
            PathInput.Text = string.Empty;
            PathInput.IsReadOnly = true;
            ProgressError_TextBox.Text = string.Empty;
            #endregion

            fileSystemManager.Notify = true;

            string[] drives = Directory.GetLogicalDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                if (drives[i].Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    fileSystemManager.Notify = false;
                }
            }

            #region renew cancellation token
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
            #endregion

            DisplayProgressBar.IsIndeterminate = true;
            CurrentPath = path;

            await Task.Run(() => GetFiles(path, cancellationTokenSource), cancellationTokenSource.Token);

            eventStopwatch.Reset();
            CurrentPath = path;
            DisplayProgressBar.IsIndeterminate = false;
            PathInput.IsReadOnly = false;
        }

        private void GetFiles(string path, CancellationTokenSource tokenSource)
        {
            IList<PathItemVM> pathItems = CurrentFiles;
            eventStopwatch.Start();
            fileSystemManager.GetFileSystemEntries(path, tokenSource.Token, ref pathItems, AddDelegate);
            Application.Current.Dispatcher.Invoke(() => SortList());
        }

        private void Next()
        {
            if (nextPathStack.Count > 0)
            {
                pathHistory.Push(CurrentPath);
                _ = DisplayFiles(nextPathStack.Pop());
            }
        }

        private void Back()
        {
            if (pathHistory.Count > 0)
            {
                nextPathStack.Push(CurrentPath);
                _ = DisplayFiles(pathHistory.Pop());
            }
        }

        private void SortList()
        {
            PathItemVM[] pathItems = new PathItemVM[CurrentFiles.Count];
            CurrentFiles.CopyTo(pathItems, 0);

            QuickSort.Sort(pathItems, 0, pathItems.Length - 1);
            CurrentFiles.Clear();
            for (int i = 0; i < pathItems.Length; i++)
            {
                CurrentFiles.Add(pathItems[i]);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void AddDelegate(IList<PathItemVM> items, IFileSystemEntry item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                items.Add(new PathItemVM(item)
                {
                    Color = new SolidColorBrush(item.IsDirectory ? Colors.Green : Colors.White)
                });
            });
        }

        #region events handlers

        #region window
        private void FolderHistory_Click(object sender, RoutedEventArgs e)
        {
            object folderName = (sender as Button)?.Content;
            if (folderName is string name)
            {
                pathHistory.Push(CurrentPath);
                _ = DisplayFiles(fileSystemManager.GetRealPath(name));
            }
        }

        private void PathInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBlock)
            {
                string text = textBlock.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    pathHistory.Push(CurrentPath);
                    string cleanText = fileSystemManager.GetRealPath(cleaner.RemoveChariotReturns(text));

                    _ = DisplayFiles(cleanText);

                    Data.History.Add(cleanText);
                    textBlock.Text = cleanText;
                }
            }

            e.Handled = true;
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is PathItemVM path)
            {
                pathHistory.Push(CurrentPath);
                _ = DisplayFiles(path.Path);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Back();
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Next();
        }

        private void PathInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (cleaner.HasForbiddenChar(e.Text))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        private void RefreshFileList_Click(object sender, RoutedEventArgs e)
        {
            fileSystemManager.Reset();
            _ = DisplayFiles(CurrentPath);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1:
                    Back();
                    break;
                case MouseButton.XButton2:
                    Next();
                    break;
            }
        }

        private void HistoryListViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Data.History.Clear();
        }
        #endregion

        #region file system manager
        private void FileSystemManager_Progress(object sender, string message)
        {
            if (sender == null)
            {
                Application.Current.Dispatcher.Invoke(() => ProgressError_TextBox.Text = message);
            }
            else if (eventStopwatch.ElapsedMilliseconds > 50) //ms interval between each notification
            {
                Application.Current.Dispatcher.Invoke(() => CurrentPath = message);

                eventStopwatch.Restart();
            }
        }

        private void FileSystemManager_Exception(object sender, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressError_TextBox.Text = exception.Message;
            });
            Console.WriteLine(exception.Message);
        }
        #endregion

        #region window chrome
        private void MultiToolWindowChrome_CloseClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void MultiToolWindowChrome_MinimizeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            WindowState = WindowState.Minimized;
        }

        private void MultiToolWindowChrome_MaximizeClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            e.Handled = true;
        }

        private void MultiToolWindowChrome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        #endregion

        #endregion
    }
}
