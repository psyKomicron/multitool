using System;
using System.IO;

namespace Multitool.FileSystem
{
    public class FileEntry : FileSystemEntry
    {
        public FileEntry(FileInfo info) : base(info) 
        {
            FileInfo = info;
            Partial = false;
        }

        public override long Size
        {
            get => FileInfo.Length;
            set
            {
                throw new InvalidOperationException("Cannot set the size of a file, property relies on the actual file system infos.");
            }
        }
    }
}
