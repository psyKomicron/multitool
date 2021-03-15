using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace BusinessLayer.FileSystem
{
    internal abstract class PathItem : IPathItem
    {
        private string _path;
        private string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract long Size { get; set; }
        public abstract FileAttributes Attributes { get; }
        public abstract bool IsHidden { get; }
        public abstract bool IsSystem { get; }
        public abstract bool IsReadOnly { get; }
        public abstract bool IsEncrypted { get; }
        public abstract bool IsCompressed { get; }
        public abstract bool IsDevice { get; }
        public abstract bool IsDirectory { get; }

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

        public PathItem() { }

        /// <summary>
        /// Refreshes the internal <see cref="FileSystemInfo"/>.
        /// </summary>
        public abstract void RefreshInfos();

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

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
