using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Multitool.FileSystem
{
    public abstract class FileSystemEntry : IFileSystemEntry
    {
        private string _path;
        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract long Size { get; set; }
        protected abstract FileSystemInfo Info { get; }

        public FileAttributes Attributes => Info.Attributes;
        public bool IsHidden => (Attributes & FileAttributes.Hidden) != 0;
        public bool IsSystem => (Attributes & FileAttributes.System) != 0;
        public bool IsReadOnly => (Attributes & FileAttributes.ReadOnly) != 0;
        public bool IsEncrypted => (Attributes & FileAttributes.Encrypted) != 0;
        public bool IsCompressed => (Attributes & FileAttributes.Compressed) != 0;
        public bool IsDevice => (Attributes & FileAttributes.Device) != 0;
        public bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;
        public string Path
        {
            get => _path;
            set 
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        protected FileSystemEntry(FileSystemInfo info)
        {
            _path = info.FullName;
            _name = info.Name;
        }

        /// <summary>
        /// Refreshes the internal <see cref="FileSystemInfo"/>.
        /// </summary>
        public void RefreshInfos()
        {
            Info.Refresh();
            SetInfos();
        }

        public int CompareTo(object obj)
        {
            if (obj is FileSystemEntry that)
            {
                return CompareTo(that);
            }
            else
            {
                return 0;
            }
        }

        public int CompareTo(IFileSystemEntry other)
        {
            if (IsDirectory && !other.IsDirectory)
            {
                return -1;
            }
            else if (!IsDirectory && other.IsDirectory)
            {
                return 1;
            }

            if (Size > other.Size)
            {
                return -1;
            }
            else if (Size < other.Size)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(IFileSystemEntry other)
        {
            return Name.Equals(other.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Set path and name of this <see cref="FileSystemEntry"/>. Use after refreshing info.
        /// </summary>
        protected void SetInfos()
        {
            Path = Info.FullName;
            Name = Info.Name;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
