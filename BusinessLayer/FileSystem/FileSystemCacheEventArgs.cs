using System;
using System.Collections.Generic;

namespace BusinessLayer.FileSystem
{
    internal class FileSystemCacheEventArgs : EventArgs
    {
        public FileSystemCacheEventArgs(string path, PathItem item, bool ttlReached): base()
        {
            Path = path;
            ItemChanged = item;
            CacheTtlReached = ttlReached;
        }

        public string Path { get; private set; }
        public PathItem ItemChanged { get; private set; }
        public bool CacheTtlReached { get; private set; }
    }
}
