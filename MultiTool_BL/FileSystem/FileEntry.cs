using System;
using System.IO;

namespace Multitool.FileSystem
{
    public class FileEntry : FileSystemEntry
    {
        private readonly FileInfo _info;

        public FileEntry(FileInfo info) : base(info)
        {
            _info = info;
            FileInfo = info;
        }

        public override long Size
        {
            get =>_info.Length;
            set
            {
                throw new InvalidOperationException("Cannot set the size of a file, property relies on the actual file system infos.");
            }
        }

        public override FileSystemInfo Info => _info;
    }
}
