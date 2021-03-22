﻿using BusinessLayer.FileSystem.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
        // read only
        private readonly DateTime creationTime;
        private readonly List<FileSystemEntry> watchedItems;
        private readonly FileSystemWatcher watcher;
        private readonly Timer timer;
        private readonly string path;
        // non read-only
        private double ttl;
        private bool frozen;

        /// <summary>
        /// Fired when the watched items underwent a change, and should be updated.
        /// </summary>
        public event CacheChangedEventHandler ItemChanged;

        /// <summary>
        /// Fired whenever the cache TTL reached 0, and thus should be updated.
        /// </summary>
        public event TTLReachedEventHandler TTLReached;

        public int Count => watchedItems.Count;
        public bool Frozen => frozen;
        public bool Partial { get; set; }
        public DateTime CreationTime => creationTime;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        /// <param name="items">Items to watch</param>
        public FileSystemCache(string path, double ttl, List<FileSystemEntry> items)
        {
            CheckPath(path);

            this.ttl = ttl;
            this.path = path;
            watchedItems = items;

            try
            {
                timer = new Timer(ttl);
                CreateTimer();

                watcher = new FileSystemWatcher(path)
                {
                    NotifyFilter = GetNotifyFilters()
                };

                BuildWatcher();
                
                timer.Start();
            }
            catch (ArgumentException e)
            {
                watchedPaths.TryTake(out string key);
                throw e;
            }
            catch (FileNotFoundException e)
            {
                watchedPaths.TryTake(out _);
                throw e;
            }

            creationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Constuctor.
        /// </summary>
        /// <param name="path">File path to monitor</param>
        /// <param name="ttl">Cache time-to-live</param>
        public FileSystemCache(string path, double ttl)
        {
            CheckPath(path);

            watchedItems = new List<FileSystemEntry>(10);
            this.ttl = ttl;
            this.path = path;

            try
            {
                timer = new Timer(ttl);
                CreateTimer();

                watcher = new FileSystemWatcher(path)
                {
                    NotifyFilter = GetNotifyFilters()
                };

                BuildWatcher();
            }
            catch (ArgumentException e)
            {
                watchedPaths.TryTake(out _);
                throw e;
            }
            catch (FileNotFoundException e)
            {
                watchedPaths.TryTake(out _);
                throw e;
            }

            creationTime = DateTime.UtcNow;
        }

        public FileSystemEntry this[int index]
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

        public void UnFroze()
        {
            timer.Interval = ttl;
            frozen = false;
        }

        /// <summary>
        /// Add an <see cref="FileSystemEntry"/> to the internal collection.
        /// </summary>
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

        /// <summary>
        /// Remove a <see cref="FileSystemEntry"/> from the collection.
        /// </summary>
        /// <param name="item"><see cref="FileSystemEntry"/> to remove</param>
        /// <returns><see cref="true"/> if the item was removed, <see cref="false"/> if not</returns>
        public bool Remove(FileSystemEntry item)
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
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, ttl, true));
        }

        /// <summary>
        /// Use to discard the cache.
        /// </summary>
        public void Delete()
        {
            frozen = true;
            watchedItems.Clear();
            watchedPaths.TryTake(out _);
            timer.Stop();
            watcher.EnableRaisingEvents = false;
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

        private void BuildWatcher()
        {
            //watcher.Changed += OnFileChange;
            watcher.Created += OnFileCreated;
            watcher.Deleted += OnFileDeleted;
            watcher.Renamed += OnFileRenamed;
            watcher.Error += OnWatcherError;

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
            TTLReached?.Invoke(this, new TTLReachedEventArgs(path, this, ttl));
        }

        #region watcher events
        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();
            FileSystemEntry item = watchedItems.Find(v => v.Path == e.FullPath);
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

            FileSystemEntry item = watchedItems.Find(v => v.Path.Equals(e.FullPath, StringComparison.OrdinalIgnoreCase));
            watchedItems.Remove(item);

            ItemChanged?.Invoke(this, new FileSystemCacheEventArgs(path, item, false, WatcherChangeTypes.Deleted));
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (frozen)
            {
                return;
            }

            ResetTimer();

            FileSystemEntry item = watchedItems.Find(v => v.Path.Equals(e.OldFullPath, StringComparison.OrdinalIgnoreCase));

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
