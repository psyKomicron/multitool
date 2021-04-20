using System;
using System.IO;

namespace MultiToolBusinessLayer.FileSystem.Events
{
    internal class FileSystemCacheEventArgs : EventArgs
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="path">Where the item is from</param>
        /// <param name="item"><see cref="FileSystemEntry"/> changed</param>
        /// <param name="ttlReached"><see cref="true"/> if the event was raised because the cache TTL reached 0</param>
        public FileSystemCacheEventArgs(string path, FileSystemEntry item, bool ttlReached, WatcherChangeTypes changeTypes): base()
        {
            Path = path;
            ItemChanged = item;
            TTLReached = ttlReached;
            ChangeTypes = changeTypes;
        }

        public FileSystemCacheEventArgs(string path, FileSystemEntry item, bool ttlReached) : base()
        {
            Path = path;
            ItemChanged = item;
            TTLReached = ttlReached;
        }

        /// <summary>
        /// Path of the item.
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// Item to be changed.
        /// </summary>
        public FileSystemEntry ItemChanged { get; private set; }

        /// <summary>
        /// true if the event was raised because the cache TTL reached 0.
        /// </summary>
        public bool TTLReached { get; private set; }

        public WatcherChangeTypes ChangeTypes { get; private set; }
    }
}
