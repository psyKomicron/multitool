using System.IO;

namespace Multitool.FileSystem
{
    internal class DirectoryEntry : FileSystemEntry
    {
        private readonly DirectoryInfo _info;
        private long _size;

        public override long Size
        {
            get => _size;
            set
            {
                _size = value;
                NotifyPropertyChanged();
            }
        }

        protected override FileSystemInfo Info => _info;

        public DirectoryEntry(DirectoryInfo info, long size) : base(info)
        {
            _info = info;
            _size = size;
        }
    }
}
