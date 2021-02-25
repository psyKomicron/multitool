using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BusinessLayer.FileSystem
{
    public class PathItem : IComparable<PathItem>, IComparable, INotifyPropertyChanged, IEquatable<PathItem>
    {
        private string _path;
        private long _size;
        private string _name;
        private FileAttributes _attributes;

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

        public FileAttributes Attributes
        {
            get => _attributes;
            set
            {
                _attributes = value;
            }
        }

        public bool IsHidden => _attributes.HasFlag(FileAttributes.Hidden);

        public bool IsSystem => _attributes.HasFlag(FileAttributes.System);

        public bool IsReadOnly => _attributes.HasFlag(FileAttributes.ReadOnly);

        public bool IsEncrypted => _attributes.HasFlag(FileAttributes.Encrypted);

        public bool IsCompressed => _attributes.HasFlag(FileAttributes.Compressed);

        public bool IsDevice => _attributes.HasFlag(FileAttributes.Device);

        public bool IsDirectory => _attributes.HasFlag(FileAttributes.Directory);

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

        public int CompareTo(PathItem other)
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(PathItem other)
        {
            return Name.Equals(other.Name);
        }
    }
}
