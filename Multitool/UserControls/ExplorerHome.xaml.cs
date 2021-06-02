using Multitool.FileSystem;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for ExplorerHome.xaml
    /// </summary>
    public partial class ExplorerHome : UserControl, INotifyPropertyChanged
    {
        private const double heightAnimationDelta = 7;
        private const double widthAnimationDelta = 5;
        private string[] sysFiles = new string[] { "pagefile.sys", "hiberfil.sys", "swapfile.sys" };
        private string _recycleBinSize = string.Empty;
        private double _recycleBinPercentage;
        private double _sysFilesPercentage;
        private long _sysFilesSize;
        private DirectorySizeCalculator calculator = new DirectorySizeCalculator();
        private Stopwatch stopwatch = new Stopwatch();

        public ExplorerHome(DriveInfo driveInfo, CancellationTokenSource cancelToken)
        {
            DriveInfo = driveInfo;
            InitializeComponent();
            cancelToken.Token.ThrowIfCancellationRequested();
            _ = LoadComponents(cancelToken);
        }

        #region properties
        public DriveInfo DriveInfo { get; set; }
        public string DriveName => DriveInfo.Name + "(" + DriveInfo?.VolumeLabel + ")";
        public string DriveCapacity => FormatSize(DriveInfo?.TotalSize ?? 0);
        public string DriveFreeSpace => FormatSize(DriveInfo?.TotalFreeSpace ?? 0);
        public string SysFilesSize => FormatSize(_sysFilesSize);
        public double DriveFreeSpacePercentage
        {
            get
            {
                if (DriveInfo != null)
                {
                    return (DriveInfo.TotalFreeSpace / (double)DriveInfo.TotalSize) * 100;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string RecycleBinSize
        {
            get => _recycleBinSize;
            private set
            {
                _recycleBinSize = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecycleBinSize)));
            }
        }
        public double RecycleBinPercentage
        {
            get => _recycleBinPercentage;
            private set
            {
                _recycleBinPercentage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RecycleBinPercentage)));
            }
        }
        public double SysFilesPercentage
        {
            get => _sysFilesPercentage;
            set
            {
                _sysFilesPercentage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SysFilesPercentage)));
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        #region methods
        private async Task LoadComponents(CancellationTokenSource cancelTokenSource)
        {
            if (DriveInfo == null) 
                return;

            CancellationToken cancelToken = cancelTokenSource.Token;
            cancelToken.ThrowIfCancellationRequested();

            FileSystemManager manager = FileSystemManager.Get();
            long size = await Task.Run(() => 
            {
                return calculator.CalculateDirectorySize(DriveInfo.Name + @"$RECYCLE.BIN\", cancelToken);
            }, cancelToken);

            Application.Current.Dispatcher.Invoke(() =>
            {
                RecycleBinSize = FormatSize(size);
                RecycleBinPercentage = size / (double)DriveInfo.TotalSize * 100;
            });
            RecycleBin_TextBlock.Opacity = 1;

            cancelToken.ThrowIfCancellationRequested();

            await GetStaticSysFilesSize(cancelToken);
            SysFiles_TextBlock.Opacity = 1;
            cancelTokenSource.Dispose();
        }

        private async Task GetStaticSysFilesSize(CancellationToken cancelToken)
        {
            await Task.Run(() =>
            {
                stopwatch.Start();

                for (int i = 0; i < sysFiles.Length; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (File.Exists(DriveInfo.Name + sysFiles[i]))
                    {
                        _sysFilesSize += new FileInfo(DriveInfo.Name + sysFiles[i]).Length;
                    }
                }
                DisplaySysFileSize();
            }, cancelToken);

            cancelToken.ThrowIfCancellationRequested();
            await Task.Run(() => ComputeSysFiles(DriveInfo.Name, cancelToken), cancelToken); 
        }

        private void ComputeSysFiles(string path, CancellationToken cancelToken)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                for (int i = 0; i < dirs.Length; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    ComputeSysFiles(dirs[i], cancelToken);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }

            try
            {
                string[] files = Directory.GetFiles(path);
                FileInfo fileInfo;
                for (int i = 0; i < files.Length; i++)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    if (files[i].EndsWith(".sys"))
                    {
                        try
                        {
                            fileInfo = new FileInfo(files[i]);
                            _sysFilesSize += fileInfo.Length;
                            DisplaySysFileSize();
                        }
                        catch (UnauthorizedAccessException) { }
                        catch (FileNotFoundException) { }
                    }
                }
            } 
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        private void DisplaySysFileSize()
        {
            if (stopwatch.ElapsedMilliseconds > 150)
            {
                stopwatch.Reset();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SysFilesSize)));
                    SysFilesPercentage = _sysFilesSize / (double)DriveInfo.TotalSize * 100;
                });

                stopwatch.Start();
            }
        }

        private string FormatSize(long size)
        {
            if (size >= (long)Sizes.TERA)
            {
                return Math.Round(size / (double)Sizes.TERA, 3) + " Tb";
            }
            if (size >= (long)Sizes.GIGA)
            {
                return Math.Round(size / (double)Sizes.GIGA, 3) + " Gb";
            }
            if (size >= (long)Sizes.MEGA)
            {
                return Math.Round(size / (double)Sizes.MEGA, 3) + " Mb";
            }
            if (size >= (long)Sizes.KILO)
            {
                return Math.Round(size / (double)Sizes.KILO, 3) + " Kb";
            }
            return size + " b";
        }
        #endregion

        #region events
        private void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Control_Loaded;
            double controlHeight, controlWidth;
            controlHeight = ActualHeight;
            controlWidth = ActualWidth;

            Height = controlHeight;
            Width = controlWidth;

            HeightMouseEnter_Animation.To = controlHeight + heightAnimationDelta;
            HeightMouseLeave_Animation.To = controlHeight;
            WidthMouseEnter_Animation.To = controlWidth + widthAnimationDelta;
            WidthMouseLeave_Animation.To = controlWidth;
        }

        private void ClearTrashBin_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion
    }

    enum Sizes : long
    {
        TERA = 1000000000000,
        GIGA = 1000000000,
        MEGA = 1000000,
        KILO = 1000
    }
}
