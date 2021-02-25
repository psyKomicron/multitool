using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace BusinessLayer.FileSystem
{
    /// <summary>
    /// Provides logic to watch a directory for changes, and to signal the changes
    /// after duplicating them to it's stored <see cref="PathItem"/>.
    /// </summary>
    internal class FileSystemCache
    {
        private static ConcurrentBag<string> watchedPaths = new ConcurrentBag<string>();

        private readonly Timer timer;
        private double ttl;
        private string path;
        private List<PathItem> watchedItems;

        public event CacheChangedEventHandler CacheItemsChanged;

        public int Count => watchedItems.Count;

        public FileSystemCache(string path, double ttl, List<PathItem> items)
        {
            CheckPath(path);

            watchedPaths.Add(path);

            this.ttl = ttl;
            timer = new Timer(ttl);
            timer.Elapsed += OnTTLReached;
            timer.AutoReset = true;

            this.path = path;
            watchedItems = items;

            FileSystemWatcher watcher = new FileSystemWatcher(path);
            watcher.Changed += OnFileChange;
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnWatcherError;

            timer.Start();
        }

        public FileSystemCache(string path, double ttl)
        {
            CheckPath(path);

            this.path = path;

            this.ttl = ttl;
            timer = new Timer(ttl);
            watchedItems = new List<PathItem>(10);
        }

        public PathItem this[int index]
        {
            get => watchedItems[index];
        }

        public void Add(PathItem item)
        {
            if (!watchedItems.Contains(item))
            {
                watchedItems.Add(item);
            }
        }

        public bool Remove(PathItem item)
        {
            return watchedItems.Remove(item);
        }

        /// <summary>
        /// Changes the time to live (TTL) value for the cache. Changing the value will act as if the TTL was reached.
        /// </summary>
        /// <param name="newTtl"></param>
        public void UpdateTTL(double newTtl)
        {
            ttl = newTtl;
            TTLReached();
        }

        private void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileSystemCacheException(path + " does not exist. Please provide a valid path.");
            }

            if (watchedPaths.Contains(path))
            {
                throw new FileSystemCacheException(path + " is already being monitored.");
            }

            if (File.Exists(path))
            {
                throw new FileSystemCacheException(path + " is a file, please provide a directory to watch.");
            }
        }

        private void TTLReached()
        {
            watchedItems.Clear();
            CacheItemsChanged?.Invoke(this, new FileSystemCacheEventArgs(path, null, true));
        }

        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            // reset timer
            timer.Interval = ttl;

            // get file changed
            string fullPath = e.FullPath;
            string fileName = e.Name;

            PathItem item = watchedItems.Find(v => v.Path == fullPath);

            if (item != null)
            {
                CacheItemsChanged?.Invoke(this, new FileSystemCacheEventArgs(path, item, false));
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {

        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {

        }

        private void OnFileRenamed(object sender, FileSystemEventArgs e)
        {

        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // handle error and if not handled, throw error
            throw new FileSystemCacheException("FileSystemWatcher raised error : \n" + e.ToString());
        }

        private void OnTTLReached(object sender, ElapsedEventArgs e) => TTLReached();

    }
}
