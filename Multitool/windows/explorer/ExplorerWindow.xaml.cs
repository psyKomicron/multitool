using BusinessLayer;
using BusinessLayer.FileSystem;
using BusinessLayer.Parsers;
using MultiTool.Tools;
using MultiTool.ViewModels;
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
        private readonly UriCleaner cleaner = new UriCleaner();
        private string _currentPath;
        private string nextPath = string.Empty;
        private uint counter;
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
            fileSystemManager.Notify = true;
            CurrentFiles.Clear();
            path = fileSystemManager.GetRealPath(path);

            string[] drives = Directory.GetLogicalDrives();
            for (int i = 0; i < drives.Length; i++)
            {
                if (drives[i].Equals(path, StringComparison.OrdinalIgnoreCase))
                {
                    fileSystemManager.Notify = false;
                }
            }

            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();

            DisplayProgressBar.IsIndeterminate = true;
            PathInput.IsReadOnly = true;
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
            fileSystemManager.GetFileSystemEntries(path, tokenSource.Token, ref pathItems, AddDelegate);
            Application.Current.Dispatcher.Invoke(() => SortList());
        }

        /// <summary>
        /// Loads the previously visited directory.
        /// </summary>
        private void Next()
        {
            if (!string.IsNullOrEmpty(nextPath))
            {
                _ = DisplayFiles(nextPath);
            }
        }

        /// <summary>
        /// Loads the current directory's parent.
        /// </summary>
        private void Back()
        {
            nextPath = CurrentPath;
            DirectoryInfo parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                _ = DisplayFiles(parent.FullName);
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

        #region Events handlers

        #region calling asynchronous
        private void FolderHistory_Click(object sender, RoutedEventArgs e)
        {
            object folderName = (sender as Button)?.Content;
            if (folderName is string name)
            {
                _ = DisplayFiles(name);
            }
        }

        private void PathInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox textBlock)
            {
                string text = textBlock.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    string cleanText = cleaner.RemoveChariotReturns(text);

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
                _ = DisplayFiles(path.Path);
            }
        }

        private void Previous_Click(object sender, RoutedEventArgs e)
        {
            Back();
            e.Handled = true;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Next();
            e.Handled = true;
        }
        #endregion

        private void FileSystemManager_Progress(object sender, string message)
        {
            if (!eventStopwatch.IsRunning)
            {
                eventStopwatch.Start();
            }
            else if (counter > 30 && eventStopwatch.ElapsedMilliseconds < 100)
            {
                eventStopwatch.Reset();
                fileSystemManager.Notify = false;
                Application.Current.Dispatcher.Invoke(() => PathInput.Text = "Progress update notifications muted for performance reasons.");
            }

            if (sender == null)
            {
                Application.Current.Dispatcher.Invoke(() => ProgressError_TextBox.Text = message);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => CurrentPath = message);
            }
            counter++;
        }

        private void FileSystemManager_Exception(object sender, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressError_TextBox.Text = exception.Message;
            });
            Console.WriteLine(exception.Message);
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

        private void MultiToolWindowChrome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void HistoryListViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Data.History.Clear();
        }
        #endregion
    }
}
