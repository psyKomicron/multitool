using BusinessLayer.FileSystem;
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
using System.Timers;
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
        private UriCleaner cleaner = new UriCleaner();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Working { get; private set; }

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

        /// <summary>
        /// Display the files to the window.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private async Task DisplayFiles(string path)
        {
            // Cancel tasks running in the background to remove cpu load and strange behavior
            if (Working)
            {
                cancellationTokenSource.Cancel(); // cancel previous tasks
                //cancellationTokenSource.Token.Register()
            }

            CurrentPath = path;
            DisplayProgressBar.IsIndeterminate = true;
            CurrentFiles.Clear();


            Working = true;

            try
            {
                await Task.Run(() => GetFiles(path, cancellationTokenSource), cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                cancellationTokenSource.Dispose();
                //cancellationTokenSource = new CancellationTokenSource();
            }

            Working = false;
            RunInUIThread(() => DisplayProgressBar.IsIndeterminate = false);
        }

        private void GetFiles(string path, CancellationTokenSource tokenSource)
        {
            FileSystemManager manager = FileSystemManager.Get();

            foreach (PathItem item in manager.GetEnumeratorFiles(path))
            {
                if (cancellationTokenSource.Token.IsCancellationRequested)
                {
                    RunInUIThread(() => DisplayProgressBar.IsIndeterminate = false);
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }

                RunInUIThread(() =>
                {
                    CurrentFiles.Add(
                        new PathItemVM(item)
                        {
                            Color = new SolidColorBrush(item.IsDirectory ? Colors.Green : Colors.White)
                        }
                        );
                });
            }
        }

        #region Events handlers

        #region calling asynchronous
        private void FolderHistory_Click(object sender, RoutedEventArgs e)
        {
            object folderName = (sender as Button)?.Content;
            if (folderName is string name)
            {
                DisplayFiles(name);
            }
        }

        private void PathInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (e.Source as TextBox);
            if (textBox != null && textBox.Text.Length - 1 >= 0 && textBox.Text[textBox.Text.Length - 1] == 10)
            {
                string cleanText = cleaner.RemoveChariotReturns(textBox.Text);

                DisplayFiles(cleanText);

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
                DisplayFiles(path.Path);
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
            FileSystemManager.Get().ClearDirectoryCache(CurrentPath);
            DisplayFiles(CurrentPath);
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
        #endregion

        /// <summary>
        /// Loads the previously visited directory.
        /// </summary>
        private void Next()
        {
            if (!string.IsNullOrEmpty(nextPath))
            {
                DisplayFiles(nextPath);
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
                DisplayFiles(parent.FullName);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RunInUIThread(Action action)
        {
            Application.Current.Dispatcher.Invoke(action);
        }

        private void CheckCancellation(CancellationTokenSource cancellationToken)
        {
            if (cancellationToken.Token.IsCancellationRequested)
            {
                RunInUIThread(() => DisplayProgressBar.IsIndeterminate = false);
                cancellationToken.Token.ThrowIfCancellationRequested();
            }
        }
    }
}
