using Multitool.FileSystem;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MultiTool.ViewModels
{
    public class FileSystemEntryViewModel : IFileSystemEntry
    {
        private const string HIDDEN = "👁";
        private const string SYSTEM = "⚙";
        private const string READONLY = "❌";
        private const string ENCRYPTED = "🔒";
        private const string COMPRESSED = "💾";
        private const string DEVICE = "‍💻";

        private readonly IFileSystemEntry pathItem;
        private string[] units = new string[] { "b", "Kb", "Mb", "Gb", "Tb" };
        private Brush _color;
        private string _displaySizeUnit;

        /// <summary>Constructor.</summary>
        /// <param name="item"><see cref="IFileSystemEntry"/> to decorate</param>
        public FileSystemEntryViewModel(IFileSystemEntry item)
        {
            pathItem = item;
            item.PropertyChanged += OnItemPropertyChanged;
        }

        #region properties

        #region IFileSystemEntry
        public FileSystemInfo Info { get => pathItem.Info; }
        public string Path => pathItem.Path;
        public long Size => pathItem.Size;
        public string Name => pathItem.Name;
        public FileAttributes Attributes => pathItem.Attributes;
        public bool IsHidden => pathItem.IsHidden;
        public bool IsSystem => pathItem.IsSystem;
        public bool IsReadOnly => pathItem.IsReadOnly;
        public bool IsEncrypted => pathItem.IsEncrypted;
        public bool IsCompressed => pathItem.IsCompressed;
        public bool IsDevice => pathItem.IsDevice;
        public bool IsDirectory => pathItem.IsDirectory;
        #endregion

        public IFileSystemEntry FileSystemEntry => pathItem;

        public string IsHiddenEcon => pathItem.IsHidden ? HIDDEN : string.Empty;

        public string IsSystemEcon => pathItem.IsSystem ? SYSTEM : string.Empty;

        public string IsReadOnlyEcon => pathItem.IsReadOnly ? READONLY : string.Empty;

        public string IsEncryptedEcon => pathItem.IsEncrypted ? ENCRYPTED : string.Empty;

        public string IsCompressedEcon => pathItem.IsCompressed ? COMPRESSED : string.Empty;

        public string IsDeviceEcon => pathItem.IsDevice ? DEVICE : string.Empty;

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
            get
            {
                if (Size >= 0)
                {
                    int unit = 0;
                    decimal currentSize = Size;

                    while (currentSize > 1024 && unit != 4)
                    {
                        currentSize /= 1024;
                        unit++;
                    }

                    DisplaySizeUnit = units[unit];

                    return currentSize.ToString("F2", CultureInfo.InvariantCulture);
                }
                else
                {
                    return string.Empty;
                } 
            }
        }

        public string DisplaySizeUnit
        {
            get => Size >= 0 ? _displaySizeUnit : string.Empty;
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
            return pathItem.CompareTo(other);
        }

        ///<inheritdoc/>
        public int CompareTo(object obj)
        {
            return pathItem.CompareTo(obj);
        }

        ///<inheritdoc/>
        public bool Equals(IFileSystemEntry other)
        {
            return pathItem.Equals(other);
        }

        ///<inheritdoc/>
        public void Delete()
        {
            pathItem.Delete();
        }

        ///<inheritdoc/>
        public void Rename(string newName)
        {
            pathItem.Rename(newName);
        }

        ///<inheritdoc/>
        public void Move(string newPath)
        {
            pathItem.Move(newPath);
        }

        ///<inheritdoc/>
        public void CopyTo(string newPath)
        {
            pathItem.CopyTo(newPath);
        }
        #endregion

        #region private
        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Size))
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplaySize)));
            }
            else
            {
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
