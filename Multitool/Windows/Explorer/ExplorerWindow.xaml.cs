using Multitool.FileSystem;
using Multitool.FileSystem.Completion;
using Multitool.FileSystem.Events;
using Multitool.Parsers;
using Multitool.Sorting;
using MultitoolWPF.ViewModels;

using MultitoolWPF.Tools;
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
using Microsoft.Win32;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for ExplorerWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window, ISerializableWindow, INotifyPropertyChanged
    {
        private static SolidColorBrush RED = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush WHITE = new SolidColorBrush(Colors.White);

        private string _currentPath;
        // tri-state: [ignored, home, explorer, editor]
        private byte loaded;
        private CancellationTokenSource homeCancellationToken;
        private CancellationTokenSource fsCancellationTokenSource;
        private FileSystemManager fileSystemManager;
        private IPathCompletor pathCompletor;
        private Stopwatch eventStopwatch = new Stopwatch();
        private Stopwatch taskStopwatch = new Stopwatch();
        private Stack<string> previousStackPath = new Stack<string>(10);
        private Stack<string> nextPathStack = new Stack<string>(10);
        private UriCleaner cleaner = new UriCleaner();

        public ExplorerWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        #region properties

        public ExplorerWindowData Data { get; set; }
        public ObservableCollection<FileSystemEntryViewModel> CurrentFiles { get; private set; }
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
            Data = WindowManager.PreferenceManager.GetWindowData<ExplorerWindowData>(Name);
            CurrentPath = Data.LastUsedPath;

            fileSystemManager = Data.TTL != default ? new FileSystemManager(Data.TTL, true) : new FileSystemManager();
            fileSystemManager.Progress += FileSystemManager_Progress;
            fileSystemManager.Exception += FileSystemManager_Exception;
            fileSystemManager.Completed += FileSystemManager_Completed;
            fileSystemManager.Change += FileSystemManager_Change;
        }

        public void Serialize()
        {
            Data.LastUsedPath = CurrentPath;
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }
        #endregion

        #region private

        private void InitializeWindow()
        {
            DataContext = this;
            CurrentFiles = new ObservableCollection<FileSystemEntryViewModel>();
            pathCompletor = new PathCompletor();
            PathAutoCompletion = new ObservableCollection<string>();
        }

        private void DisplayHome()
        {
            homeCancellationToken = new CancellationTokenSource();
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
                    Height = new GridLength(280)
                });
            }

            for (int i = 0; i < driveInfo.Length; i++)
            {
                CancellationTokenSource homeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(homeCancellationToken.Token);

                ExplorerHome home = new ExplorerHome(driveInfo[i], homeCancellationTokenSource);
                home.MouseDoubleClick += ExplorerHomeControl_MouseDoubleClick;
                home.Height = home.MinHeight;
                home.Width = home.MinWidth;

                Grid.SetRow(home, i / 2);
                Grid.SetColumn(home, i % 2 == 0 ? 0 : 1);
                Disks_Grid.Children.Add(home);
            }

            loaded |= 0b100;
        }

        private void DisplayFiles(string path)
        {
            loaded |= 0b10;

            if (fsCancellationTokenSource != null)
            {
                fsCancellationTokenSource.Cancel();
                fsCancellationTokenSource.Dispose();
            }
            fsCancellationTokenSource = new CancellationTokenSource();

            try
            {
                string realPath = fileSystemManager.GetRealPath(cleaner.RemoveChariotReturns(path));
                CurrentPath = realPath;
                if (realPath.Length < 10 && !Data.History.Contains(realPath))
                {
                    Data.History.Add(realPath);
                }
                else if (realPath.Length >= 10 && !Data.History.Contains(string.Format("{0, 10}", realPath)))
                {
                    Data.History.Add(string.Format("{0, 10}", realPath));
                }

                PathAutoCompletion.Clear();
                CurrentFiles.Clear();
                Progress_TextBox.Text = string.Empty;
                fileSystemManager.Notify = Files_ProgressBar.IsIndeterminate = CancelAction_Button.IsEnabled = true;

                IList<FileSystemEntryViewModel> pathItems = CurrentFiles;
                eventStopwatch.Restart();
                taskStopwatch.Restart();

                try
                {
                    fileSystemManager.GetFileSystemEntries(realPath, fsCancellationTokenSource.Token, CurrentFiles, AddDelegate);
                }
                catch (ArgumentException argExcep)
                {
                    eventStopwatch.Reset();
                    CancelAction_Button.IsEnabled = false;
                    Files_ProgressBar.IsIndeterminate = false;

                    Console.Error.WriteLine(argExcep);
                    Progress_TextBox.Text = argExcep.Message;
                }
            }
            catch (DirectoryNotFoundException)
            {
                eventStopwatch.Reset();
                CancelAction_Button.IsEnabled = false;
                Files_ProgressBar.IsIndeterminate = false;

                CurrentPath = path;
                path = path.ToLowerInvariant();
                for (int i = 0; i < Data.History.Count; i++)
                {
                    if (Data.History[i].Equals(path, StringComparison.OrdinalIgnoreCase))
                    {
                        Data.History.RemoveAt(i);
                        break;
                    }
                }
                Progress_TextBox.Text = "Directory not found (" + path + ").";
            }
        }

        private void Next()
        {
            if (nextPathStack.Count > 0)
            {
                previousStackPath.Push(CurrentPath);
                DisplayFiles(nextPathStack.Pop());
            }
        }

        private void Back()
        {
            if (previousStackPath.Count > 0)
            {
                nextPathStack.Push(CurrentPath);
                DisplayFiles(previousStackPath.Pop());
            }
            else
            {
                nextPathStack.Push(CurrentPath);
                DisplayFiles(Directory.GetParent(CurrentPath).FullName);
            }
        }

        private void SortList()
        {
            FileSystemEntryViewModel[] pathItems =  ObservableCollectionQuickSort.Sort(CurrentFiles);
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

        private void AddDelegate(IList<FileSystemEntryViewModel> items, IFileSystemEntry item)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                items.Add(new FileSystemEntryViewModel(item));
            });
        }

        private async void DisplayMessage(string message, bool error = false, bool force = false)
        {
            await Task.Run(() =>
            {
                if (force || eventStopwatch.ElapsedMilliseconds > 20) //ms interval between each notification
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

        #endregion

        #region events handlers

        #region window

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            if (!string.IsNullOrWhiteSpace(textBlock.Text))
            {
                string toolTip;
                if (!string.IsNullOrEmpty(Properties.Resources.ResourceManager.GetString(textBlock.Text)))
                {
                    toolTip = Properties.Resources.ResourceManager.GetString(textBlock.Text);
                }
                else
                {
                    string ext = Path.GetExtension(textBlock.Text);

                    toolTip = (!string.IsNullOrEmpty(ext) && !string.IsNullOrEmpty(Properties.Resources.ResourceManager.GetString(ext)))
                        ? Properties.Resources.ResourceManager.GetString(ext)
                        : "Unknown";
                }
                
                if (textBlock.ToolTip == null)
                {
                    textBlock.ToolTip = new ToolTip();
                }
                textBlock.ToolTip = toolTip;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //exceptionWindow.Close();
            try
            {
                homeCancellationToken.Cancel();
                homeCancellationToken.Dispose();
            }
            catch (ObjectDisposedException) { }

            if (fsCancellationTokenSource != null && fileSystemManager != null)
            {
                fileSystemManager.Notify = false;
                try
                {
                    fsCancellationTokenSource.Cancel();
                    fsCancellationTokenSource.Dispose();
                }
                catch (ObjectDisposedException) { }
            }
            eventStopwatch.Stop();

            WindowState = WindowState.Normal;
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

        private void PathInput_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = PathInput.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    DisplayFiles(text);
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
                DisplayFiles(name);
            }
        }

        private void MainListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = ((ListView)e.Source).SelectedItem;

            if (item != null && item is FileSystemEntryViewModel fsEntry)
            {
                if (fsEntry.IsDirectory)
                {
                    DisplayFiles(fsEntry.Path);
                }
                else
                {
                    try
                    {
                        _ = Process.Start(fsEntry.Path);
                    }
                    catch (Win32Exception w32e)
                    {
                        Console.WriteLine(w32e.ToString());
                        // fsEntry.Color = new SolidColorBrush(Colors.Red);
                    }
                }
                e.Handled = true;
            }
        }

        private void HistoryListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is string path)
            {
                e.Handled = true;
                DisplayFiles(path);
            }
        }

        private void EditorListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = (e.Source as ListView)?.SelectedItem;

            if (item != null && item is FileSystemEntryViewModel fsEntry)
            {
                if (!fsEntry.IsDirectory)
                {
                    if (Registry.ClassesRoot.OpenSubKey(Path.GetExtension(fsEntry.Path)) != null)
                    {
                        MainListView_MouseDoubleClick(sender, e);
                    }
                    else
                    {
                        _ = Dispatcher.BeginInvoke((Action)(() => Window_TabControl.SelectedIndex = 2));
                    }
                }
            }
            e.Handled = true;
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
            DisplayFiles(CurrentPath);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (fsCancellationTokenSource != null)
            {
                fsCancellationTokenSource.Cancel();
            }
            e.Handled = true;
        }

        private void HistoryListViewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Data.History.Clear();
        }

        private void ExplorerHomeControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DriveInfo info = ((ExplorerHome)sender).DriveInfo;
            DisplayFiles(info.Name);
            _ = Dispatcher.BeginInvoke((Action)(() => Window_TabControl.SelectedIndex = 1));
        }

        private void PathItemDelete_Click(object sender, RoutedEventArgs e)
        {
            System.Collections.IList items = MainListView.SelectedItems;
            foreach (object item in items)
            {
                if (item is FileSystemEntryViewModel pathItem)
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

        private void WindowTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (loaded != 0b111)
            {
                switch (Window_TabControl.SelectedIndex)
                {
                    case 0:
                        if ((loaded & 0b100) == 0)
                        {
                            DisplayHome();
                        }
                        break;
                    case 1:
                        if ((loaded & 0b10) == 0)
                        {
                            DisplayFiles(CurrentPath);
                        }
                        break;
                    case 2:
                        if ((loaded & 0b1) == 0)
                        {
                            TextEditor editor = new TextEditor()
                            {
                                Height = 100,
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top
                            };
                            editor.DrawingPosition = editor.TranslatePoint(new Point(0, 0), EditorGrid);

                            Grid.SetColumn(editor, 1);
                            Grid.SetRow(editor, 0);
                            _ = EditorGrid.Children.Add(editor);
                            loaded |= 0b1;
                        }
                        break;
                }
            }
        }

        #endregion

        #region manager

        private void FileSystemManager_Progress(object sender, string message)
        {
            DisplayMessage(message, false, sender == null);
        }

        private void FileSystemManager_Exception(object sender, Exception exception)
        {
            DisplayMessage(exception.Message, true);
            //exceptionWindow.Queue(exception);
        }

        private void FileSystemManager_Change(object sender, ChangeEventArgs data)
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
                    Console.WriteLine(data.Entry.Path + " changed");
                    break;
                case WatcherChangeTypes.Renamed:
                    Console.WriteLine(data.Entry.Path + " renamed");
                    break;
                case WatcherChangeTypes.All:
                    Console.WriteLine(data.Entry.Path + " : all changes");
                    break;
            }
            data.InUse = false;
        }

        private void FileSystemManager_Completed(TaskStatus status, Task task)
        {
            taskStopwatch.Stop();
            fsCancellationTokenSource.Dispose();
            fsCancellationTokenSource = null;
            eventStopwatch.Reset();

            Dispatcher.Invoke(() =>
            {
                CancelAction_Button.IsEnabled = false;
                Files_ProgressBar.IsIndeterminate = false;
                SortList();

                string message = string.Empty;
                switch (status)
                {
                    case TaskStatus.RanToCompletion:
                        message = "Task successfully completed";
                        break;
                    case TaskStatus.Canceled:
                        message = "Task cancelled";
                        break;
                    case TaskStatus.Faulted:
                        message = "Task failed\n";
                        Console.Error.WriteLine(task.Exception.ToString());
                        break;
                }

                Progress_TextBox.Text = taskStopwatch.Elapsed.TotalSeconds > 0
                    ? message + " (in " + Math.Round(taskStopwatch.Elapsed.TotalSeconds).ToString() + "s)"
                    : message + " (in " + taskStopwatch.ElapsedMilliseconds.ToString() + "ms)";
            });
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

        #endregion

        #endregion
    }
}
