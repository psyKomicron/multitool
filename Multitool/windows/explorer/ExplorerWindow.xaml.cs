using MultiTool.Tools;
using MultiTool.ViewModels;
using Multitool.FileSystem;
using Multitool.Parsers;
using Multitool.Sorting;
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
using MultitoolWPF.UserControls;

namespace MultiTool.Windows
{
    /// <summary>
    /// Interaction logic for ExplorerWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window, ISerializableWindow, INotifyPropertyChanged
    {
        private static readonly SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush WHITE = new SolidColorBrush(Colors.White);
        private string _currentPath;
        private UriCleaner cleaner = new UriCleaner();
        private Stack<string> pathHistory = new Stack<string>(10);
        private Stack<string> nextPathStack = new Stack<string>(10);
        private FileSystemManager fileSystemManager;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Stopwatch eventStopwatch = new Stopwatch();

        public ExplorerWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        #region properties
        public ExplorerWindowData Data { get; set; }
        public ObservableCollection<PathItemViewModel> CurrentFiles { get; private set; }
        public string CurrentPath
        {
            get => _currentPath;
            set
            {
                _currentPath = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        #region serialize
        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowManager<ExplorerWindowData>(Name);
            CurrentPath = Data.LastUsedPath;

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
            _ = DisplayFiles(CurrentPath);
        }

        public void Serialize()
        {
            Data.LastUsedPath = CurrentPath;
            WindowManager.PreferenceManager.AddWindowManager(Data, Name);
        }
        #endregion

        #region private
        private void InitializeWindow()
        {
            DataContext = this;
            CurrentFiles = new ObservableCollection<PathItemViewModel>();
        }

        private void LoadHome()
        {
            DriveInfo[] driveInfo = DriveInfo.GetDrives();
            for (int i = 0; i < driveInfo.Length; i++)
            {
                Disks_StackPanel.Children.Add(new ExplorerHome(driveInfo[i]));
            }
        }

        private async Task DisplayFiles(string path)
        {
            #region clear displays
            CurrentFiles.Clear();
            PathInput.Text = string.Empty;
            Progress_TextBox.Text = string.Empty;
            #endregion
            #region renew cancellation token
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                //cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
            #endregion

            fileSystemManager.Notify = true;
            DisplayProgressBar.IsIndeterminate = true;
            CurrentPath = path;
            CancelAction_Button.IsEnabled = true;

            try
            {
                await Task.Run(() => GetFiles(path, cancellationTokenSource), cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Progress_TextBox.Text = "Operation cancelled";
            }
            finally
            {
                eventStopwatch.Reset();
                CancelAction_Button.IsEnabled = false;
                CurrentPath = path;
                DisplayProgressBar.IsIndeterminate = false;
            }
        }

        private void GetFiles(string path, CancellationTokenSource tokenSource)
        {
            IList<PathItemViewModel> pathItems = CurrentFiles;
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
            PathItemViewModel[] pathItems =  ObservableCollectionQuickSort.Sort(CurrentFiles);
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

        private void AddDelegate(IList<PathItemViewModel> items, IFileSystemEntry item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                items.Add(new PathItemViewModel(item)
                {
                    Color = item.IsDirectory ? Tool.GetRessource<SolidColorBrush>("DevBlue") : new SolidColorBrush(Colors.White)
                });
            });
        }
        #endregion

        #region events handlers

        #region window
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (cancellationTokenSource != null && fileSystemManager != null)
            {
                fileSystemManager.Notify = false;
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadHome();
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
                    pathHistory.Push(string.Format("{0, 10}", CurrentPath));
                    string cleanText = fileSystemManager.GetRealPath(cleaner.RemoveChariotReturns(text));

                    _ = DisplayFiles(cleanText);

                    Data.History.Add(cleanText);
                    textBlock.Text = cleanText;
                }
            }

            e.Handled = true;
        }
        
        private void PathInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (cleaner.HasForbiddenChar(e.Text))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is PathItemViewModel path)
            {
                e.Handled = true;
                pathHistory.Push(CurrentPath);
                _ = DisplayFiles(path.Path);
            }
        }

        private void History_ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is string path)
            {
                e.Handled = true;
                _ = DisplayFiles(path);
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

        private void RefreshFileList_Click(object sender, RoutedEventArgs e)
        {
            fileSystemManager.Reset();
            _ = DisplayFiles(CurrentPath);
        }

        private void CancelAction_Button_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                //cancellationTokenSource.Dispose();
            }
            cancellationTokenSource = new CancellationTokenSource();
        }

        private void HistoryListViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Data.History.Clear();
        }
        #endregion

        #region file system manager
        private void DisplayProgressMessage(string message, bool error = false, bool force = false)
        {
            if (eventStopwatch.ElapsedMilliseconds > 70 || force) //ms interval between each notification
            {
                if (error)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Progress_TextBox.Foreground = RED;
                        Progress_TextBox.Text = message;
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Progress_TextBox.Foreground = WHITE;
                        Progress_TextBox.Text = message;
                    });
                }
                eventStopwatch.Restart();
            }
        }

        private void FileSystemManager_Progress(object sender, string message)
        {
            if (sender == null)
            {
                DisplayProgressMessage(message, false, true);
            }
            else 
            {
                DisplayProgressMessage(message);
            }
        }

        private void FileSystemManager_Exception(object sender, Exception exception)
        {
            DisplayProgressMessage(exception.Message, true);
            Debug.WriteLine(exception.Message);
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
