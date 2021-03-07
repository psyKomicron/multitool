using BusinessLayer.FileSystem.Events;
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
    /// Provides logic to watch a directory for changes, and to signal the changes.
    /// <see cref="FileSystemCache"/> will only signal the changes and not update itself.
    /// </summary>
    internal class FileSystemCache : IDisposable
    {
        private static readonly ConcurrentBag<string> watchedPaths = new ConcurrentBag<string>();

        private readonly Timer timer;
        private double ttl;
        private string path;
        private bool frozen;
        private List<PathItem> watchedItems;
        private FileSystemWatcher watcher;

        /// <summary>
        /// Fired when the watched items underwent a change, and should be updated.
        /// </summary>
        public event CacheChangedEventHandler ItemChanged;

        /// <summary>
        /// Fired whenever the cache TTL reached 0, and thus should be updated.
        /// </summary>
        public event TTLReachedEventHandler TTLReached;

        /// <summary>
        /// Get the number of <see cref="PathItem"/> cached.
        /// </summary>
        public int Count => watchedItems.Count;

        public bool Frozen => frozen;

        public bool Partial { get; set; }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        /// <param name="items">Items to watch</param>
        public FileSystemCache(string path, double ttl, List<PathItem> items)
        {
            CheckPath(path);

            this.ttl = ttl;
            this.path = path;
            watchedItems = items;

            try
            {
                timer = new Timer(ttl);
                CreateTimer();

                CreateWatcher(path);
                
                timer.Start();
            }
            catch (ArgumentException e)
            {
                watchedPaths.TryTake(out string key);
                throw e;
            }
            catch (FileNotFoundException e)
            {
                watchedPaths.TryTake(out string key);
                throw e;
            }
        }

        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        public FileSystemCache(string path, double ttl)
        {
            CheckPath(path);

            watchedItems = new List<PathItem>(10);
            this.ttl = ttl;
            this.path = path;

            try
            {
                timer = new Timer(ttl);
                CreateTimer();

                CreateWatcher(path);
            }
            catch (ArgumentException e)
            {
                watchedPaths.TryTake(out string key);
                throw e;
            }
            catch (FileNotFoundException e)
            {
                watchedPaths.TryTake(out string key);
                throw e;
            }
        }

        public PathItem this[int index]
        {
            get
            {
                return watchedItems[index];
            }
        }

        public void Dispose()
        {
            watcher.Dispose();
        }

        public void Reset()
        {
            timer.Interval = ttl;
            frozen = false;
        }

        /// <summary>
        /// Add an <see cref="PathItem"/> to the internal collection.
        /// </summary>
        /// <param name="item"><see cref="PathItem"/> to add</param>
        public void Add(PathItem item)
        {
            IsFrozen();

            if (!timer.Enabled)
            {
                timer.Start();
            }

            if (!watchedItems.Contains(item))
            {
                watchedItems.Add(item);
            }
        }

        /// <summary>
        /// Remove a <see cref="PathItem"/> from the collection.
        /// </summary>
        /// <param name="item"><see cref="PathItem"/> to remove</param>
        /// <returns><see cref="true"/> if the item was removed, <see cref="false"/> if not</returns>
        public bool Remove(PathItem item)
        {
            IsFrozen();
            return watchedItems.Remove(item);
        }

        /// <summary>
        /// Changes the time to live (TTL) value for the cache. Changing the value will act as if the TTL was reached.
        /// </summary>
        /// <param name="newTTL">The new TTL</param>
        public void UpdateTTL(double newTTL)
        {
            IsFrozen();
            ttl = newTTL;
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, false));
        }

        public void Refresh()
        {
            foreach (PathItem item in watchedItems)
            {
                item.Refresh();
            }
        }

        private void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileSystemCacheException(path + " does not exist. Please provide a valid path.");
            }

            foreach (string watchedPath in watchedPaths)
            {
                if (string.Equals(watchedPath, path, StringComparison.OrdinalIgnoreCase))
                {
                    throw new FileSystemCacheException(path + " is already being monitored.");
                }
            }

            if (File.Exists(path))
            {
                throw new FileSystemCacheException(path + " is a file, please provide a directory to watch.");
            }

            watchedPaths.Add(path);
        }

        private void ResetTimer()
        {
            timer.Stop();
            timer.Interval = ttl;
        }

        private void IsFrozen()
        {
            if (frozen)
            {
                throw new CacheFrozenException();
            }
        }

        private void CreateWatcher(string path)
        {
            watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = GetNotifyFilters()
            };

            #region events subscription
            //watcher.Changed += OnFileChange;
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnWatcherError;
            #endregion

            watcher.EnableRaisingEvents = true;
        }

        private void CreateTimer()
        {
            timer.Elapsed += OnTimerElapsed;
            timer.AutoReset = true;
        }

        private NotifyFilters GetNotifyFilters()
        {
            return NotifyFilters.FileName
                 | NotifyFilters.DirectoryName 
                 | NotifyFilters.Attributes
                 | NotifyFilters.Size 
                 | NotifyFilters.LastWrite
                 | NotifyFilters.CreationTime
                 | NotifyFilters.Security;
        }

        #region events
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            frozen = true;
            timer.Stop();
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, true));
        }

        #region watcher events
        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();

            PathItem item = watchedItems.Find(v => v.Path == e.FullPath);

            ItemChanged?.Invoke(this, new FileSystemCacheEventArgs(path, item, false, e.ChangeType));

            timer.Start();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();
            ItemChanged?.Invoke(this, new FileSystemCacheEventArgs(path, null, false, WatcherChangeTypes.Created));
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();

            PathItem item = watchedItems.Find(v => v.Path.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase));

            ItemChanged?.Invoke(this, new FileSystemCacheEventArgs(path, item, false, WatcherChangeTypes.Deleted));
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();

            PathItem item = watchedItems.Find(v => v.Path.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase));

            if (item != null)
            {
                item.Name = e.Name;
                item.Path = e.FullPath;
            }

            ItemChanged?.Invoke(this, new FileSystemCacheEventArgs(path, item, false, WatcherChangeTypes.Renamed));

            timer.Start();
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // handle error and if not handled, throw error
            throw new FileSystemCacheException("FileSystemWatcher raised error : \n" + e.ToString());
        }
        #endregion

        #endregion
    }
}
