using System;
using System.IO;

namespace BusinessLayer.FileSystem
{
    internal class FileEntry : FileSystemEntry
    {
        private readonly FileInfo _info;

        public override long Size
        {
            get =>_info.Length;
            set
            {
                throw new InvalidOperationException("Cannot set the size of a file, property is reliant on the actual file system infos.");
            }
        }

        protected override FileSystemInfo Info => _info;

        public FileEntry(FileInfo info) : base(info)
        {
            _info = info;
        }
    }
}
