using Multitool.FileSystem;
using Multitool.FileSystem.Completion;
using Multitool.NTInterop;
using Multitool.Parsers;
using Multitool.Sorting;

using MultiTool.Tools;
using MultiTool.ViewModels;

using MultitoolWPF.UserControls;

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
        private static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush WHITE = new SolidColorBrush(Colors.White);

        private Stopwatch eventStopwatch = new Stopwatch();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private Stack<string> pathHistory = new Stack<string>(10);
        private Stack<string> nextPathStack = new Stack<string>(10);
        private CursorPosition cursorPosition = new CursorPosition();
        private UriCleaner cleaner = new UriCleaner();
        private string _currentPath;
        private FileSystemManager fileSystemManager;
        private IPathCompletor pathCompletor;
        private bool resizing;
        private bool mouseOverBorder;
        private Point previousCursor;

        public ExplorerWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        #region properties
        public ExplorerWindowData Data { get; set; }
        public ObservableCollection<PathItemViewModel> CurrentFiles { get; private set; }
        public ObservableCollection<string> PathAutoCompletion { get; private set; }
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
            fileSystemManager.Change += FileSystemManager_Change;
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
            pathCompletor = new PathCompletor();
            PathAutoCompletion = new ObservableCollection<string>();

            MouseMove += OnMouseMove;
            MouseUp += OnMouseUp;
        }

        private void LoadHome()
        {    
            DriveInfo[] driveInfo = DriveInfo.GetDrives();

            int rows, column;
            column = driveInfo.Length > 1 ? 2 : 1;
            rows = (driveInfo.Length / 2) + (driveInfo.Length % 2);

            for (int i = 0; i < column; i++)
            {
                Disks_Grid.ColumnDefinitions.Add(new ColumnDefinition() 
                {
                    Width = new GridLength(630)
                });
            }
            for (int i = 0; i < rows; i++)
            {
                Disks_Grid.RowDefinitions.Add(new RowDefinition() 
                {
                    Height = new GridLength(230)
                });
            }

            for (int i = 0; i < driveInfo.Length; i++)
            {
                ExplorerHome home = new ExplorerHome(driveInfo[i]);
                home.MouseDoubleClick += ExplorerHomeControl_MouseDoubleClick;
                home.Height = home.MinHeight;
                home.Width = home.MinWidth;

                Grid.SetRow(home, i / 2);
                Grid.SetColumn(home, i % 2 == 0 ? 0 : 1);
                Disks_Grid.Children.Add(home);
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

        #region resize
        private void Explorer_IsMouseDirectlyOverChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true) && e.OldValue.Equals(false))
            {
                Cursor = Cursors.SizeNWSE;
                mouseOverBorder = true;
            }
            else if (!resizing)
            {
                Cursor = Cursors.Arrow;
                mouseOverBorder = false;
            }
        }

        private void WindowBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (mouseOverBorder)
            {
                resizing = true;

                previousCursor = cursorPosition.GetCursorPosition();
                CaptureMouse();
            }
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            resizing = false;
            Cursor = Cursors.Arrow;
            ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (resizing)
            {
                cursorPosition.GetCursorPosition(out int x, out int y);
                Console.WriteLine(x + ", " + y);
                double width, height;
                width = previousCursor.X - x;
                height = previousCursor.Y - y;
                if (Width + width >= MinWidth)
                    Width += width;
                if (Height + height >= MinHeight)
                    Height += height;

                previousCursor.X = x;
                previousCursor.Y = y;
            }
        }
        #endregion

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

        private void Window_Loaded(object sender, RoutedEventArgs e) => LoadHome();

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

        private void PathInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = PathInput.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    pathHistory.Push(string.Format("{0, 10}", CurrentPath));
                    string cleanText = fileSystemManager.GetRealPath(cleaner.RemoveChariotReturns(text));

                    _ = DisplayFiles(cleanText);
                    Data.History.Add(cleanText);
                    PathInput.Text = cleanText;
                    PathAutoCompletion.Clear();
                }
            }
            else
            {
                string path = PathInput.Text;
                try
                {
                    pathCompletor.Complete(path, PathAutoCompletion);
                }
                catch (UnauthorizedAccessException) { }
            }

            e.Handled = true;
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

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is PathItemViewModel path)
            {
                if (path.IsDirectory)
                {
                    pathHistory.Push(CurrentPath);
                    _ = DisplayFiles(path.Path);
                }
                else
                {
                    Process.Start(path.Path);
                }
                e.Handled = true;
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

        private void ExplorerHomeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DriveInfo info = ((ExplorerHome)sender).DriveInfo;
            _ = DisplayFiles(info.Name);
            Dispatcher.BeginInvoke((Action)(() => Window_TabControl.SelectedIndex = 1));
        }

        private void PathItemDelete_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.IList items = MainListView.SelectedItems;
            foreach (object item in items)
            {
                if (item is PathItemViewModel pathItem)
                {
                    try
                    {
                        pathItem.Delete();
                    }
                    catch (IOException err)
                    {
                        DisplayMessage(err.Message, true, true);
                    }
                }
            }
        }
        #endregion

        #region file system manager
        private async void DisplayMessage(string message, bool error = false, bool force = false)
        {
            await Task.Run(() =>
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
            });
        }

        private void FileSystemManager_Progress(object sender, string message)
        {
            if (sender == null)
            {
                DisplayMessage(message, false, true);
            }
            else
            {
                DisplayMessage(message);
            }
        }

        private void FileSystemManager_Exception(object sender, Exception exception) => DisplayMessage(exception.Message, true);

        private void FileSystemManager_Change(object sender, Multitool.FileSystem.Events.ChangeEventArgs data)
        {
            switch (data.ChangeTypes)
            {
                case WatcherChangeTypes.Created:
                    AddDelegate(CurrentFiles, data.Entry);
                    break;
                case WatcherChangeTypes.Deleted:
                    for (int i = 0; i < CurrentFiles.Count; i++)
                    {
                        if (CurrentFiles[i].FileSystemEntry.Equals(data.Entry))
                        {
                            Application.Current.Dispatcher.Invoke(() => CurrentFiles.RemoveAt(i));
                            return;
                        }
                    }
                    break;
                case WatcherChangeTypes.Changed:
                    break;
                case WatcherChangeTypes.Renamed:
                    break;
                case WatcherChangeTypes.All:
                    break;
                default:
                    break;
            }
            data.InUse = false;
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
