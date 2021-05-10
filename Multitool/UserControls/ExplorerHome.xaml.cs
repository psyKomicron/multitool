using Multitool.FileSystem;
using System;
using System.ComponentModel;
using System.IO;
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

        public ExplorerHome()
        {
            InitializeComponent();
        }

        public ExplorerHome(DriveInfo driveInfo)
        {
            DriveInfo = driveInfo;
            InitializeComponent();
            _ = LoadComponents();
        }

        #region display properties
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
        #endregion

        #region methods
        public double SysFilesPercentage
        {
            get => _sysFilesPercentage;
            set
            {
                _sysFilesPercentage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SysFilesPercentage)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task LoadComponents()
        {
            if (DriveInfo == null) return;

            FileSystemManager manager = FileSystemManager.Get();
            long size = await Task.Run(() => manager.ComputeDirectorySize(DriveInfo.Name + @"$RECYCLE.BIN\", null));
            Application.Current.Dispatcher.Invoke(() =>
            {
                RecycleBinSize = FormatSize(size);
                RecycleBinPercentage = size / (double)DriveInfo.TotalSize * 100;
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                _sysFilesSize = GetStaticSysFilesSize();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SysFilesSize)));
                SysFilesPercentage = _sysFilesSize / (double)DriveInfo.TotalSize * 100;
            });
        }

        private long GetStaticSysFilesSize()
        {
            long size = 0;
            for (int i = 0; i < sysFiles.Length; i++)
            {
                if (File.Exists(DriveInfo.Name + sysFiles[i]))
                {
                    size += new FileInfo(DriveInfo.Name + sysFiles[i]).Length;
                }
            }

            return size;
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
