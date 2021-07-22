using Multitool.FileSystem;

using MultitoolWPF.Tools;

using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MultitoolWPF.ViewModels
{
    public class FileSystemEntryViewModel : IFileSystemEntry
    {
        private static string DirectoryIcon = "📁";
        private static string FileIcon = "📄";
        private static string HiddenIcon = "👁";
        private static string SystemIcon = "⚙";
        private static string ReadOnlyIcon = "❌";
        private static string EncryptedIcon = "🔒";
        private static string CompressedIcon = "💾";
        private static string DeviceIcon = "‍💻";
        private string _displaySizeUnit;

        /// <summary>Constructor.</summary>
        /// <param name="item"><see cref="IFileSystemEntry"/> to decorate</param>
        public FileSystemEntryViewModel(IFileSystemEntry item)
        {
            FileSystemEntry = item;
            item.PropertyChanged += OnItemPropertyChanged;

            Icon = IsDirectory ? DirectoryIcon : FileIcon;

            Color = IsDirectory ? new SolidColorBrush(Tool.GetAppRessource<Color>("DevBlueColor")) : new SolidColorBrush(Colors.White);
            if (!Partial)
            {
                Tool.FormatSize(Size, out double formatted, out string ext);
                DisplaySizeUnit = ext;
                DisplaySize = formatted.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
            {
                Color.Opacity = 0.6;
                DisplaySize = string.Empty;
            }
        }

        #region properties

        #region IFileSystemEntry
        public FileSystemInfo Info => FileSystemEntry.Info;
        public string Path => FileSystemEntry.Path;
        public long Size => FileSystemEntry.Size;
        public string Name => FileSystemEntry.Name;
        public FileAttributes Attributes => FileSystemEntry.Attributes;
        public bool IsHidden => FileSystemEntry.IsHidden;
        public bool IsSystem => FileSystemEntry.IsSystem;
        public bool IsReadOnly => FileSystemEntry.IsReadOnly;
        public bool IsEncrypted => FileSystemEntry.IsEncrypted;
        public bool IsCompressed => FileSystemEntry.IsCompressed;
        public bool IsDevice => FileSystemEntry.IsDevice;
        public bool IsDirectory => FileSystemEntry.IsDirectory;
        public bool Partial => FileSystemEntry.Partial;
        #endregion

        public Brush Color { get; }
        public string DisplaySize { get; private set; }
        public IFileSystemEntry FileSystemEntry { get; }
        public string Icon { get; }
        public string IsHiddenEcon => FileSystemEntry.IsHidden ? HiddenIcon : string.Empty;
        public string IsSystemEcon => FileSystemEntry.IsSystem ? SystemIcon : string.Empty;
        public string IsReadOnlyEcon => FileSystemEntry.IsReadOnly ? ReadOnlyIcon : string.Empty;
        public string IsEncryptedEcon => FileSystemEntry.IsEncrypted ? EncryptedIcon : string.Empty;
        public string IsCompressedEcon => FileSystemEntry.IsCompressed ? CompressedIcon : string.Empty;
        public string IsDeviceEcon => FileSystemEntry.IsDevice ? DeviceIcon : string.Empty;

        public string DisplaySizeUnit
        {
            get => _displaySizeUnit;

            set
            {
                _displaySizeUnit = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        #region public
        ///<inheritdoc/>
        public int CompareTo(IFileSystemEntry other)
        {
            return FileSystemEntry.CompareTo(other);
        }

        ///<inheritdoc/>
        public int CompareTo(object obj)
        {
            return FileSystemEntry.CompareTo(obj);
        }

        ///<inheritdoc/>
        public bool Equals(IFileSystemEntry other)
        {
            return FileSystemEntry.Equals(other);
        }

        ///<inheritdoc/>
        public void Delete()
        {
            FileSystemEntry.Delete();
        }

        ///<inheritdoc/>
        public void Rename(string newName)
        {
            FileSystemEntry.Rename(newName);
        }

        ///<inheritdoc/>
        public void Move(string newPath)
        {
            FileSystemEntry.Move(newPath);
        }

        ///<inheritdoc/>
        public void CopyTo(string newPath)
        {
            FileSystemEntry.CopyTo(newPath);
        }
        #endregion

        #region private
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Size))
            {
                Tool.FormatSize(Size, out double formatted, out string ext);
                DisplaySizeUnit = ext;
                DisplaySize = formatted.ToString("F2", CultureInfo.InvariantCulture);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplaySize)));
            }
            else
            {
                if (e.PropertyName == nameof(Partial))
                {
                    Color.Opacity = 1;
                }

                PropertyChanged?.Invoke(this, e);
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
