using Multitool.FileSystem;
using Multitool.FileSystem.Events;

using MultitoolWPF.Tools;

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Media;

namespace MultitoolWPF.ViewModels
{
    public class FileSystemEntryViewModel : ViewModel, IFileSystemEntry, INotifyPropertyChanged
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
        private string _displaySize;
        private Brush _color;

        /// <summary>Constructor.</summary>
        /// <param name="item"><see cref="IFileSystemEntry"/> to decorate</param>
        public FileSystemEntryViewModel(IFileSystemEntry item)
        {
            FileSystemEntry = item;

            item.AttributesChanged += OnAttributesChanged;
            item.Deleted += OnDeleted;
            item.SizedChanged += OnSizeChanged;

            Icon = GetIcon();
            Color = IsDirectory ? new SolidColorBrush(Tool.GetAppRessource<Color>("DevBlueColor")) : new SolidColorBrush(Colors.White);

            if (!Partial)
            {
                Color.Opacity = 0.6;
                DisplaySize = string.Empty;
            }
            else
            {
                Tool.FormatSize(Size, out double formatted, out string ext);
                DisplaySizeUnit = ext;
                DisplaySize = formatted.ToString("F2", CultureInfo.InvariantCulture);
            }
        }

        #region properties

        #region IFileSystemEntry
        public FileAttributes Attributes => FileSystemEntry.Attributes;
        public IFileSystemEntry FileSystemEntry { get; }
        public FileSystemInfo Info => FileSystemEntry.Info;
        public string Path => FileSystemEntry.Path;
        public string Name => FileSystemEntry.Name;
        public bool IsHidden => FileSystemEntry.IsHidden;
        public bool IsSystem => FileSystemEntry.IsSystem;
        public bool IsReadOnly => FileSystemEntry.IsReadOnly;
        public bool IsEncrypted => FileSystemEntry.IsEncrypted;
        public bool IsCompressed => FileSystemEntry.IsCompressed;
        public bool IsDevice => FileSystemEntry.IsDevice;
        public bool IsDirectory => FileSystemEntry.IsDirectory;
        public long Size => FileSystemEntry.Size;
        public bool Partial => FileSystemEntry.Partial;
        #endregion

        public string Icon { get; }
        public string IsHiddenEcon => FileSystemEntry.IsHidden ? HiddenIcon : string.Empty;
        public string IsSystemEcon => FileSystemEntry.IsSystem ? SystemIcon : string.Empty;
        public string IsReadOnlyEcon => FileSystemEntry.IsReadOnly ? ReadOnlyIcon : string.Empty;
        public string IsEncryptedEcon => FileSystemEntry.IsEncrypted ? EncryptedIcon : string.Empty;
        public string IsCompressedEcon => FileSystemEntry.IsCompressed ? CompressedIcon : string.Empty;
        public string IsDeviceEcon => FileSystemEntry.IsDevice ? DeviceIcon : string.Empty;

        public Brush Color
        {
            get => _color;
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        public string DisplaySize
        {
            get => _displaySize;
            set
            {
                _displaySize = value;
                NotifyPropertyChanged();
            }
        }

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

        #region events
        public event EntryChangedEventHandler Deleted
        {
            add
            {
                FileSystemEntry.Deleted += value;
            }

            remove
            {
                FileSystemEntry.Deleted -= value;
            }
        }

        public event EntrySizeChangedEventHandler SizedChanged
        {
            add
            {
                FileSystemEntry.SizedChanged += value;
            }

            remove
            {
                FileSystemEntry.SizedChanged -= value;
            }
        }

        public event EntryAttributesChangedEventHandler AttributesChanged
        {
            add
            {
                FileSystemEntry.AttributesChanged += value;
            }

            remove
            {
                FileSystemEntry.AttributesChanged -= value;
            }
        }

        public event EntryRenamedEventHandler Renamed
        {
            add
            {
                FileSystemEntry.Renamed += value;
            }

            remove
            {
                FileSystemEntry.Renamed -= value;
            }
        }
        #endregion

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
        private string GetIcon()
        {
            switch (Name)
            {
                case "$RECYCLE.BIN":
                    return "🗑";
                case "desktop.ini":
                    return "🖥";
                case "swapfile.sys":
                case "hiberfil.sys":
                case "pagefile.sys":
                    return "⚙";
                default:
                    return IsDirectory ? DirectoryIcon : FileIcon;
            }
        }

        private void OnSizeChanged(IFileSystemEntry sender, long newSize)
        {
            if (!Partial)
            {
                Color.Opacity = 1;
                NotifyPropertyChanged(nameof(Partial));
            }

            Tool.FormatSize(Size, out double formatted, out string ext);
            DisplaySizeUnit = ext;
            DisplaySize = formatted.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void OnDeleted(IFileSystemEntry sender, FileChangeEventArgs e)
        {
            Color = new SolidColorBrush(Colors.Red)
            {
                Opacity = 0.8
            };
        }

        private void OnAttributesChanged(IFileSystemEntry sender, FileAttributes attributes)
        {
            switch (attributes)
            {
                case FileAttributes.ReadOnly:
                    NotifyPropertyChanged(nameof(IsReadOnly));
                    break;
                case FileAttributes.Hidden:
                    NotifyPropertyChanged(nameof(IsHidden));
                    break;
                case FileAttributes.System:
                    NotifyPropertyChanged(nameof(IsSystem));
                    break;
                case FileAttributes.Directory:
                    NotifyPropertyChanged(nameof(IsDirectory));
                    break;
                case FileAttributes.Device:
                    NotifyPropertyChanged(nameof(IsDevice));
                    break;
                case FileAttributes.Compressed:
                    NotifyPropertyChanged(nameof(IsCompressed));
                    break;
                case FileAttributes.Encrypted:
                    NotifyPropertyChanged(nameof(IsEncrypted));
                    break;
            }
        }
        #endregion
    }
}
