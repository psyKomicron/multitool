﻿using Multitool.FileSystem.Events;
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
                    _ = watchedPaths.Remove(path);
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

        public FileSystemEntry this[int index] => watchedItems[index];

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

            lock (_lock)
            {
                if (!timer.Enabled)
                {
                    timer.Start();
                }
                if (!watchedItems.Contains(item))
                {
                    watchedItems.Add(item);
                }
            }
        }

        /// <summary>Remove a <see cref="FileSystemEntry"/> from the collection.</summary>
        /// <param name="item"><see cref="FileSystemEntry"/> to remove</param>
        /// <returns><see cref="true"/> if the item was removed, <see cref="false"/> if not</returns>
        public bool Remove(FileSystemEntry item)
        {
            IsFrozen();
            lock (_lock)
            {
                return watchedItems.Remove(item);
            }
        }

        public void RemoveAt(int i)
        {
            IsFrozen();
            lock (_lock)
            {
                watchedItems.RemoveAt(i);
            }
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

            lock (_lock)
            {
                watchedItems.Clear();
                _ = watchedPaths.Remove(Path);
            }

            timer.Stop();
            watcher.EnableRaisingEvents = false;
        }

        #region private methods
        private void CheckAndAddPath(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException(path + " does not exist. Please provide a valid path.", new DirectoryNotFoundException());
            }
            if (File.Exists(path))
            {
                throw new ArgumentException(path + " is a file, please provide a directory to watch.", new FileNotFoundException());
            }

            lock (_lock)
            {
                foreach (string watchedPath in watchedPaths)
                {
                    if (string.Equals(watchedPath, path, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException(path + " is already being monitored.");
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
                throw new InvalidOperationException("Cache is frozen.");
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
            if (!frozen)
            {
                ResetTimer();
                FileSystemEntry item = watchedItems.Find(v => v.Path == e.FullPath);
                ItemChanged?.Invoke(this, string.Empty, item, false, e.ChangeType);
                timer.Start();
            }
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (!frozen)
            {
                ResetTimer();
                ItemChanged?.Invoke(this, e.FullPath, null, false, WatcherChangeTypes.Created);
            }
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (!frozen)
            {
                ResetTimer();

                FileSystemEntry deletedItem = watchedItems.Find(v => v.Path.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase));
                _ = watchedItems.Remove(deletedItem);
                ItemChanged?.Invoke(this, e.FullPath, deletedItem, false, WatcherChangeTypes.Deleted);
            }
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (!frozen)
            {
                ResetTimer();

                FileSystemEntry item = watchedItems.Find(v => v.Path.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    item.Name = e.Name;
                    item.Path = e.FullPath;
                }

                ItemChanged?.Invoke(this, e.FullPath, item, false, WatcherChangeTypes.Renamed);
                timer.Start();
            }
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
