using BusinessLayer.Parsers;
using MultiTool.Tools;
using MultiTool.ViewModels;
using MultiTool.Windows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for ExplorerWindow.xaml
    /// </summary>
    public partial class ExplorerWindow : Window, ISerializableWindow, INotifyPropertyChanged
    {
        private string _currentPath;
        private string nextPath = string.Empty;
        private Dictionary<string, List<PathItemVM>> cache = new Dictionary<string, List<PathItemVM>>(50);
        private UriCleaner cleaner = new UriCleaner();
        private CancellationTokenSource cancellationTokenSource;

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
        }

        public void Serialize()
        {
            WindowManager.GetPreferenceManager().AddWindowManager(Data, Name);
        }

        private void InitializeWindow()
        {
            DataContext = this;
            CurrentFiles = new ObservableCollection<PathItemVM>();
        }

        private async Task DisplayFiles(string path)
        {
            // Cancel tasks running in the background to remove cpu load and strange behavior
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel(); // cancel previous tasks
                cancellationTokenSource.Dispose(); // dispose token
            }
            cancellationTokenSource = new CancellationTokenSource(); // renew token

            await Task.Run(() =>
            {
                RunInUIThread(() =>
                {
                    CurrentPath = path;
                    DisplayProgressBar.IsIndeterminate = true;
                    CurrentFiles.Clear();
                });

                if (cache.ContainsKey(path))
                {
                    List<PathItemVM> cachedData = GetFromCache(path);
                    for (int i = 0; i < cachedData.Count; i++)
                    {
                        CheckCancellation(cancellationTokenSource.Token);

                        Application.Current.Dispatcher.Invoke(() => CurrentFiles.Add(cachedData[i]));
                    }
                }
                else
                {
                    GetFiles(path, cancellationTokenSource.Token);
                }

                RunInUIThread(() => DisplayProgressBar.IsIndeterminate = false);
            },
            cancellationTokenSource.Token);
        }

        private void GetFiles(string path, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (Directory.Exists(path))
                {
                    try
                    {
                        string[] dirs = Directory.GetDirectories(path);

                        for (int i = 0; i < dirs.Length; i++)
                        {
                            CheckCancellation(cancellationToken);

                            long size = ComputeDirectorySize(dirs[i], cancellationToken);

                            RunInUIThread(() =>
                            {
                                CurrentFiles.Add(new PathItemVM()
                                {
                                    Path = dirs[i],
                                    Color = new SolidColorBrush(Colors.Green),
                                    Size = size,
                                    Name = new DirectoryInfo(dirs[i]).Name
                                });
                            });
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Debug.WriteLine(nameof(UnauthorizedAccessException));
                    }

                    try
                    {
                        SortedList<long, PathItemVM> sortedList = new SortedList<long, PathItemVM>();
                        //sortedList.Add()
                        string[] files = Directory.GetFiles(path);

                        for (int i = 0; i < files.Length; i++)
                        {
                            // Check cancel
                            CheckCancellation(cancellationToken);

                            FileInfo fileInfo = new FileInfo(files[i]);

                            RunInUIThread(() =>
                            {
                                CurrentFiles.Add(new PathItemVM()
                                {
                                    Path = files[i],
                                    Color = new SolidColorBrush(Colors.White),
                                    Size = fileInfo.Length,
                                    Name = fileInfo.Name
                                });
                            });
                        }

                        RunInUIThread(() =>
                        {
                            IOrderedEnumerable<PathItemVM> enumerable = CurrentFiles.OrderBy(pathitem =>
                            {
                                return pathitem.Size;
                            });
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Debug.WriteLine(nameof(UnauthorizedAccessException));
                    }
                }
            }

            BuildCache(path);
        }

        private void BuildCache(string path)
        {
            if (!cache.ContainsKey(path))
            {
                List<PathItemVM> items = new List<PathItemVM>();
                for (int i = 0; i < CurrentFiles.Count; i++)
                {
                    PathItemVM item = CurrentFiles[i];
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }

                cache.Add(path, items);
            }
        }

        private List<PathItemVM> GetFromCache(string path)
        {
            List<PathItemVM> cachedItems = cache[path];

            if (cachedItems.Count > 0)
            {
                string[] dirs = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);

                if (dirs.Length + files.Length == cachedItems.Count)
                {
                    return cachedItems;
                }
                else
                {
                    if (cache.Remove(path))
                    {
                        Debug.WriteLine("Successfully removed cached path " + path);
                    }
                    else
                    {
                        Debug.WriteLine("Unsucessful try to remove cached path " + path);
                    }

                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        private long ComputeDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    size += ComputeDirectorySize(subDirPath, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException) { }

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateFiles(path);

                foreach (string subDirPath in subDirPaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    try
                    {
                        size += new FileInfo(subDirPath).Length;
                    }
                    catch (FileNotFoundException) { }
                }
            }
            catch (UnauthorizedAccessException) { }

            return size;
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

        private void PathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (e.Source as TextBox);
            if (textBox != null && textBox.Text.Length - 1 >= 0 && textBox.Text[textBox.Text.Length - 1] == 10)
            {
                string cleanText = cleaner.RemoveChariotReturns(textBox.Text);

                _ = DisplayFiles(cleanText);

                Data.History.Add(cleanText);
                textBox.Text = cleanText;
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
            nextPath = CurrentPath;
            DirectoryInfo parent = Directory.GetParent(CurrentPath);
            if (parent != null)
            {
                _ = DisplayFiles(parent.FullName);
            }

            e.Handled = true;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(nextPath))
            {
                _ = DisplayFiles(nextPath);
            }

            e.Handled = true;
        }
        #endregion

        private void PathInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (cleaner.HasForbiddenChar(e.Text))
            {
                e.Handled = true;
            }
            base.OnPreviewTextInput(e);
        }
        #endregion

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RunInUIThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        private void CheckCancellation(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
