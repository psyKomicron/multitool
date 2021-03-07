using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BusinessLayer.FileSystem
{
    public class PathItem : IPathItem
    {
        private string _path;
        private long _size;
        private string _name;
        private FileSystemInfo info;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Path
        {
            get => _path;
            set
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }

        public long Size
        {
            get => _size;
            set
            {
                _size = value;
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

        public FileAttributes Attributes => info.Attributes;

        public bool IsHidden => info.Attributes.HasFlag(FileAttributes.Hidden);

        public bool IsSystem => info.Attributes.HasFlag(FileAttributes.System);

        public bool IsReadOnly => info.Attributes.HasFlag(FileAttributes.ReadOnly);

        public bool IsEncrypted => info.Attributes.HasFlag(FileAttributes.Encrypted);

        public bool IsCompressed => info.Attributes.HasFlag(FileAttributes.Compressed);

        public bool IsDevice => info.Attributes.HasFlag(FileAttributes.Device);

        public bool IsDirectory => info.Attributes.HasFlag(FileAttributes.Directory);

        public PathItem() { }

        public PathItem(string path, long size, string name, FileAttributes attributes)
        {
            _path = path;
            _size = size;
            _name = name;
            _attributes = attributes;
        }

        public int CompareTo(object obj)
        {
            if (obj is PathItem that)
            {
                return CompareTo(that);
            }
            else
            {
                return 0;
            }
        }

        public int CompareTo(IPathItem other)
        {
            if (other.Size > Size)
            {
                return 1;
            }
            else if (other.Size < Size)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        public bool Equals(IPathItem other)
        {
            return Name.Equals(other.Name);
        }

        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Refreshes the internal <see cref="FileSystemInfo"/> and fires the "update" event.
        /// </summary>
        public void Refresh()
        {
            info.Refresh();
            NotifyPropertyChanged("");
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
