using Multitool.FileSystem.Events;
using Multitool.FileSystem.Factory;

using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;

namespace Multitool.FileSystem
{
    /// <summary>
    /// Provides logic to watch a directory for changes, and to signal the changes.
    /// <see cref="FileSystemCache"/> will only signal the changes and not update itself.
    /// </summary>
    internal class FileSystemCache : IDisposable
    {
        private static List<string> watchedPaths = new List<string>();
        private static object _lock = new object();
        private DateTime creationTime;
        private List<FileSystemEntry> watchedItems;
        private FileSystemWatcher watcher;
        private Timer timer;
        private string path;
        private double ttl;
        private bool frozen;

        /// <summary>Contructor.</summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        /// <param name="items">Items to watch</param>
        public FileSystemCache(string path, double ttl, List<FileSystemEntry> items)
        {
            CheckAndAddPath(path);

            this.ttl = ttl;
            this.path = path;
            watchedItems = items;
            try
            {
                watcher = WatcherFactory.CreateWatcher(path, new WatcherDelegates()
                {
                    ChangedHandler = OnFileChange,
                    CreatedHandler = OnFileCreated,
                    DeletedHandler = OnFileDeleted,
                    RenamedHandler = OnFileRenamed
                });
                watcher.EnableRaisingEvents = true;
                CreateTimer();
                timer.Start();
                creationTime = DateTime.UtcNow;
            }
            catch (Exception e)
            {
                lock (_lock)
                {
                    watchedPaths.Remove(path);
                }
                timer.Stop();
                throw e;
            }
        }

        /// <summary>Constuctor.</summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        public FileSystemCache(string path, double ttl)
        {
            CheckAndAddPath(path);

            Partial = true;
            this.ttl = ttl;
            this.path = path;
            watchedItems = new List<FileSystemEntry>(10);
            
            try
            {
                watcher = WatcherFactory.CreateWatcher(path, new WatcherDelegates()
                {
                    ChangedHandler = OnFileChange,
                    CreatedHandler = OnFileCreated,
                    DeletedHandler = OnFileDeleted,
                    RenamedHandler = OnFileRenamed
                });
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception e) // catching exception because we are re-throwing it
            {
                lock (_lock)
                {
                    watchedPaths.Remove(path);
                }
                throw e;
            }

            CreateTimer();
            timer.Start();
            creationTime = DateTime.UtcNow;
        }

        /// <summary>Gets the internal item count.</summary>
        public int Count => watchedItems.Count;
        
        /// <summary>Tells if the cache allow operations on it or not (true if no operation are allowed).</summary>
        public bool Frozen => frozen;
        
        /// <summary>True when the cache is not complete.</summary>
        public bool Partial { get; set; }
        
        /// <summary>Gets the creation time of the cache.</summary>
        public DateTime CreationTime => creationTime;

        public string Path => path;

        /// <summary>Fired when the watched items underwent a change, and should be updated.
        public event CacheChangedEventHandler ItemChanged;
        /// <summary>Fired whenever the cache TTL reached 0, and thus should be updated.</summary>
        public event TTLReachedEventHandler TTLReached;
        public event WatcherErrorEventHandler WatcherError;

        public FileSystemEntry this[int index]
        {
            get
            {
                return watchedItems[index];
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            watcher.Dispose();
        }

        /// <summary>Unfroze the <see cref="FileSystemCache"/> to re-allow operations on it.</summary>
        public void UnFroze()
        {
            timer.Interval = ttl;
            frozen = false;
        }

        /// <summary>Add an <see cref="FileSystemEntry"/> to the internal collection.</summary>
        /// <param name="item"><see cref="FileSystemEntry"/> to add</param>
        public void Add(FileSystemEntry item)
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

        /// <summary>Remove a <see cref="FileSystemEntry"/> from the collection.</summary>
        /// <param name="item"><see cref="FileSystemEntry"/> to remove</param>
        /// <returns><see cref="true"/> if the item was removed, <see cref="false"/> if not</returns>
        public bool Remove(FileSystemEntry item)
        {
            IsFrozen();
            return watchedItems.Remove(item);
        }

        public void RemoveAt(int i)
        {
            IsFrozen();
            watchedItems.RemoveAt(i);
        }

        /// <summary>Changes the time to live (TTL) value for the cache. Changing the value will act as if the TTL was reached.</summary>
        /// <param name="newTTL">The new TTL</param>
        public void UpdateTTL(double newTTL)
        {
            IsFrozen();
            ttl = newTTL;
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, ttl, true));
        }

        /// <summary>Use to discard the cache.</summary>
        public void Delete()
        {
            frozen = true;
            watchedItems.Clear();

            lock (_lock)
            {
                watchedPaths.Remove(Path);
            }

            timer.Stop();
            watcher.EnableRaisingEvents = false;
        }

        #region private methods
        private void CheckAndAddPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FileSystemCacheException(path + " does not exist. Please provide a valid path.", new DirectoryNotFoundException());
            }
            if (File.Exists(path))
            {
                throw new FileSystemCacheException(path + " is a file, please provide a directory to watch.", new FileNotFoundException());
            }

            lock (_lock)
            {
                foreach (string watchedPath in watchedPaths)
                {
                    if (string.Equals(watchedPath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FileSystemCacheException(path + " is already being monitored.");
                    }
                }
                watchedPaths.Add(path);
            }
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

        private void CreateTimer()
        {
            timer = new Timer(ttl);
            timer.Elapsed += OnTimerElapsed;
            timer.AutoReset = true;
        }
        #endregion

        #region events
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            frozen = true;
            timer.Stop();
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, ttl));
        }

        #region watcher events
        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            if (frozen) return;
            ResetTimer();
            FileSystemEntry item = watchedItems.Find(v => v.Path == e.FullPath);
            ItemChanged?.Invoke(this, string.Empty, item, false, e.ChangeType);
            timer.Start();
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (frozen) return;
            ResetTimer();
            ItemChanged?.Invoke(this, e.FullPath, null, false, WatcherChangeTypes.Created);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (frozen) return;

            ResetTimer();

            FileSystemEntry deletedItem = watchedItems.Find(v => v.Path.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase));
            watchedItems.Remove(deletedItem);
            ItemChanged?.Invoke(this, e.FullPath, deletedItem, false, WatcherChangeTypes.Deleted);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (frozen) return;

            ResetTimer();

            FileSystemEntry item = watchedItems.Find(v => v.Path.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Name = e.FullPath;
                item.Path = e.FullPath;
            }

            ItemChanged?.Invoke(this, e.FullPath, item, false, WatcherChangeTypes.Renamed);
            timer.Start();
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            // handle error and if not handled, throw error
            // throw new FileSystemCacheException("FileSystemWatcher raised an error : \n" + e.GetException().ToString());
            if (e.GetException() != null)
            {
                if (e.GetException().InnerException == null)
                {
                    WatcherError?.Invoke(this, e.GetException(), WatcherErrorTypes.PathDeleted);
                }
                else
                {
                    throw e.GetException();
                }
            }
            else
            {
                throw new Exception("Watcher error");
            }
        }
        #endregion

        #endregion
    }
}
