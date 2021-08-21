using Multitool.FileSystem.Events;
using Multitool.Optimisation;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multitool.FileSystem
{
    /// <summary>
    /// Class to manage <see cref="IFileSystemEntry"/> with cache and async methods.
    /// </summary>
    public class FileSystemManager : IFileSystemManager
    {
#if DEBUG
        public const double DEFAULT_CACHE_TIMEOUT = 120_000;
#else
        public const double DEFAULT_CACHE_TIMEOUT = 300_000;
#endif
        public const bool DEFAULT_NOTIFY_STATUS = false;

        private static Dictionary<string, FileSystemCache> cache = new Dictionary<string, FileSystemCache>();
        private static ObjectPool<FileChangeEventArgs> objectPool = new ObjectPool<FileChangeEventArgs>();
        private object _eventlock = new object();
        private DirectorySizeCalculator calculator = new DirectorySizeCalculator();
        private double ttl;
        private bool _notify;

        /// <summary>
        /// Default constructor with default cache TTL and with <see cref="Notify"/> set to false.
        /// </summary>
        public FileSystemManager()
        {
            ttl = DEFAULT_CACHE_TIMEOUT;
            Notify = DEFAULT_NOTIFY_STATUS;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ttl"></param>
        /// <param name="notifyProgress"></param>
        public FileSystemManager(double ttl, bool notifyProgress)
        {
            this.ttl = ttl;
            Notify = notifyProgress;
        }

        #region properties
        public bool Notify
        {
            get => _notify;

            set
            {
                calculator.Notify = value;
                _notify = value;
            }
        }

        public double CacheTimeout
        {
            get => ttl;

            set
            {
                ttl = value;
                foreach (KeyValuePair<string, FileSystemCache> c in cache)
                {
                    c.Value.UpdateTTL(value);
                }
            }
        }
        #endregion

        #region events
        /// <inheritdoc/>
        public event ItemChangedEventHandler Change;

        /// <inheritdoc/>
        public event TaskCompletedEventHandler Completed;
        /// <inheritdoc/>
        public event TaskFailedEventHandler Exception
        {
            add
            {
                lock (_eventlock)
                {
                    SelfException += value;
                    calculator.Exception += value;
                }
            }
            remove
            {
                lock (_eventlock)
                {
                    calculator.Exception -= value;
                    SelfException -= value;
                }
            }
        }
        /// <inheritdoc/>
        public event TaskProgressEventHandler Progress;

        private event TaskFailedEventHandler SelfException;
        #endregion

        #region public
        /// <inheritdoc/>
        public void GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, AddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry
        {
            #region not null
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list), "List cannot be null.");
            }
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken), "CancellationToken cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A path needs to be provided (path was either null, empty, or with spaces only). Provided path: " + path, nameof(path));
            }
            #endregion

            if (ContainsKey(path))
            {
                FileSystemCache fileCache = GetFileSystemCache(path);

                if (fileCache.Frozen)
                {
                    cache.Remove(fileCache.Path);
                    fileCache.Delete();
                    fileCache.Dispose();
                    GetAll(path, cancellationToken, list, addDelegate, new FileSystemCache(path, CacheTimeout));
                }
                else
                {
                    for (int i = 0; i < fileCache.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        addDelegate(list, fileCache[i]);
                    }
                    if (fileCache.Partial)
                    {
                        GetPartial(path, fileCache, list, addDelegate, cancellationToken);
                    }
                    InvokeCompletion(TaskStatus.RanToCompletion);
                }
            }
            else if (Directory.Exists(path))
            {
                try
                {
                    GetAll(path, cancellationToken, list, addDelegate, new FileSystemCache(path, CacheTimeout));
                }
                catch (InvalidOperationException e)
                {
                    Debug.WriteLine("Unable to create cache. Exception:\n" + e.ToString());
                }
            }
        }

        /// <inheritdoc/>
        public string GetRealPath(string path)
        {
            string realPath;
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                List<string> parents = new List<string>(5);

                DirectoryInfo parent = directoryInfo.Parent;

                if (parent != null)
                {
                    IEnumerable<DirectoryInfo> directories = parent.EnumerateDirectories();
                    foreach (DirectoryInfo fileInfo in directories)
                    {
                        if (fileInfo.Name.Equals(directoryInfo.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            parents.Add(fileInfo.Name);
                            break;
                        }
                    }

                    while (parent != null && parent.Parent != null)
                    {
                        if (parent.Parent != null)
                        {
                            directories = parent.Parent.EnumerateDirectories();
                            foreach (DirectoryInfo fileInfo in directories)
                            {
                                if (fileInfo.Name.Equals(parent.Name, StringComparison.OrdinalIgnoreCase))
                                {
                                    parents.Add(fileInfo.Name);
                                    break;
                                }
                            }
                        }

                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        parents.Add(parent.Name.ToUpperInvariant());
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = parents.Count - 1; i >= 0; i--)
                    {
                        stringBuilder.Append(parents[i]);
                        if (!parents[i].Contains(Path.DirectorySeparatorChar.ToString()) && i != 0)
                        {
                            stringBuilder.Append(Path.DirectorySeparatorChar);
                        }
                    }

                    realPath = stringBuilder.ToString();
                }
                else
                {
                    return path;
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Directory not found, path : " + path);
            }
            return realPath;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            foreach (KeyValuePair<string, FileSystemCache> pair in cache)
            {
                FileSystemCache cache = pair.Value;
                cache.Delete();
            }
            cache.Clear();
        }
        #endregion

        #region private

        #region file get
        private void GetAll<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, AddDelegate<ItemType> addDelegate, FileSystemCache fileCache) where ItemType : IFileSystemEntry
        {
            Debug.WriteLine("Adding and building cache for: " + path);
            cache.Add(path, fileCache);
            fileCache.ItemChanged += OnCacheItemChanged;
            fileCache.TTLReached += OnCacheTTLReached;
            fileCache.WatcherError += OnWatcherError;

            GetFiles(path, fileCache, list, addDelegate, cancellationToken);
            _ = GetDirectories(Directory.GetDirectories(path), fileCache, list, addDelegate, cancellationToken)
                .ContinueWith((Task previous) =>
                {
                    fileCache.Partial = previous.Status != TaskStatus.RanToCompletion;
                    InvokeCompletion(previous);
                });
        }

        private void GetPartial<T>(string path, FileSystemCache cacheItems, IList<T> list, AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                List<string> paths = new List<string>(Directory.GetFileSystemEntries(path));
                List<string> toDo = new List<string>(paths.Count - cacheItems.Count + 1);
                // get partial items
                for (int i = 0; i < cacheItems.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    IFileSystemEntry cacheItem = cacheItems[i];
                    if (cacheItem.Partial)
                    {
                        toDo.Add(cacheItem.Path);
                        cacheItems.RemoveAt(i);
                        paths.Remove(cacheItem.Path);
                    }
                }
                // get the missing file entries
                for (int i = 0; i < paths.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string filePath = paths[i];
                    bool contains = false;
                    for (int j = 0; j < cacheItems.Count; j++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (cacheItems[j].Path.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            contains = true;
                            break;
                        }
                    }

                    if (!contains)
                    {
                        toDo.Add(filePath);
                    }
                }

                FileSystemEntry item;
                for (int i = 0; i < toDo.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    InvokeProgress(toDo[i]);

                    if (File.Exists(toDo[i]))
                    {
                        FileInfo fileInfo = new FileInfo(toDo[i]);
                        item = new FileEntry(fileInfo);

                        cacheItems.Add(item);
                        addDelegate(list, item);
                    }
                    else if (Directory.Exists(toDo[i]))
                    {
                        long size = calculator.CalculateDirectorySize(toDo[i], cancellationToken);
                        DirectoryInfo info = new DirectoryInfo(toDo[i]);
                        item = new DirectoryEntry(info, size);

                        cacheItems.Add(item);
                        addDelegate(list, item);
                    }
                }
                toDo.Clear();
                cacheItems.Partial = false;
            }
            catch (UnauthorizedAccessException e)
            {
                InvokeException(e);
            }
        }

        private void GetFiles<T>(string path, FileSystemCache cacheItems, IList<T> list, AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] files = Directory.GetFiles(path);
                for (int i = 0; i < files.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    InvokeProgress(files[i]);

                    FileSystemEntry item = new FileEntry(new FileInfo(files[i]));
                    cacheItems.Add(item);
                    addDelegate(list, item);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                InvokeException(e);
            }
        }

        private async Task GetDirectories<T>(string[] dirPaths, FileSystemCache cacheItems, IList<T> list, AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                List<Task> tasks = new List<Task>(dirPaths.Length);
                for (int i = 0; i < dirPaths.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    InvokeProgress(dirPaths[i]);

                    string currentPath = dirPaths[i];
                    FileSystemEntry item = new DirectoryEntry(new DirectoryInfo(currentPath));
                    tasks.Add(RunDirsParallel(item, currentPath, list, addDelegate, cancellationToken));
                    cacheItems.Add(item);
                }

                await Task.WhenAll(tasks);
            }
            catch (UnauthorizedAccessException e)
            {
                InvokeException(e);
            }
        }

        private async Task RunDirsParallel<T>(FileSystemEntry item, string currentPath, IList<T> list, AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            addDelegate(list, item);
            try
            {
                await new DirectorySizeCalculator().CalculateDirectorySizeAsync(currentPath, cancellationToken, (long newValue) => item.Size += newValue);
                item.Partial = false;
            }
            catch (AggregateException e)
            {
                e.Data.Add(GetType(), "Uncommon aggregate exception (from calculating dir size). Path :" + currentPath);
                InvokeException(e);
            }
        }
        #endregion

        private FileSystemCache GetFileSystemCache(string key)
        {
            if (cache.TryGetValue(key, out FileSystemCache sysCache))
            {
                return sysCache;
            }
            else
            {
                foreach (KeyValuePair<string, FileSystemCache> pair in cache)
                {
                    if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        return pair.Value;
                    }
                }
            }
            return null;
        }

        private bool ContainsKey(string key)
        {
            foreach (KeyValuePair<string, FileSystemCache> pair in cache)
            {
                if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        /*private FileSystemEntry GetItemFromCache(string cachePath, string itemPath)
        {
            FileSystemCache systemCache = cache[cachePath];
            if (systemCache != null)
            {
                for (int i = 0; i < systemCache.Count; i++)
                {
                    if (itemPath.Equals(systemCache[i].Path, StringComparison.OrdinalIgnoreCase))
                    {
                        return systemCache[i];
                    }
                }
            }
            return null;
        }

        private List<FileSystemEntry> GetAffectedItems(string path)
        {
            List<FileSystemEntry> itemsToChange = new List<FileSystemEntry>(50);
            DirectoryInfo dir = Directory.GetParent(path);

            if (cache.ContainsKey(dir.FullName))
            {
                itemsToChange.Add(GetItemFromCache(dir.FullName, path));

                DirectoryInfo parent = Directory.GetParent(dir.FullName);

                // if the directory is cached, else no need to update
                if (cache.ContainsKey(parent.FullName))
                {
                    while (parent != null)
                    {
                        if (cache.ContainsKey(parent.FullName))
                        {
                            FileSystemEntry item = GetItemFromCache(parent.FullName, dir.FullName);
                            itemsToChange.Add(item);
                        }
                        // get parent
                        dir = parent;
                        parent = Directory.GetParent(dir.FullName);
                    }
                }
            }

            return itemsToChange;
        }*/

        #endregion

        #region events
        private void OnCacheTTLReached(object sender, TTLReachedEventArgs e)
        {
            if (e.TTLUpdated)
            {
                return;
            }

            Trace.WriteLine("Cache TTL reached, updating " + e.Cache.Path);

            FileSystemCache fsCache = e.Cache;
            for (int i = 0; i < fsCache.Count; i++)
            {
                FileSystemEntry item = fsCache[i];
                Trace.WriteLine("\t-> updating " + item.Name);
                item.RefreshInfos();
                if (item.IsDirectory)
                {
                    item.Size = calculator.CalculateDirectorySize(item.Path, CancellationToken.None);
                }
            }
            fsCache.UnFreeze();
        }

        private void OnCacheItemChanged(object sender, string path, FileSystemEntry entry, bool ttl, WatcherChangeTypes changes)
        {
            if (changes == WatcherChangeTypes.Created)
            {
                if (Directory.Exists(path)) // Check if the added item is a directory
                {
                    Trace.WriteLine("Cache changed: directory created, computing directory size");
                    long size = calculator.CalculateDirectorySize(path, CancellationToken.None);
                    entry = new DirectoryEntry(new DirectoryInfo(path), size);
                }
                else
                {
                    entry = new FileEntry(new FileInfo(path));
                }
                ((FileSystemCache)sender).Add(entry);
            }
            Change?.Invoke(this, objectPool.GetObject(entry, changes));
        }

        private void OnWatcherError(FileSystemCache sender, Exception e, WatcherErrorTypes errType)
        {
            if (errType == WatcherErrorTypes.PathDeleted)
            {
                if (cache.ContainsKey(sender.Path))
                {
                    cache.Remove(sender.Path);
                    sender.Delete();
                }
            }
        }
        
        #endregion

        #region event invoke
        private void InvokeException(Exception e)
        {
            if (Notify)
            {
                SelfException?.Invoke(this, e);
            }
        }

        private void InvokeProgress(string message)
        {
            if (Notify)
            {
                Progress?.Invoke(this, message);
            }
        }

        private void InvokeCompletion(Task task)
        {
            if (Notify)
            {
                Completed?.Invoke(task.Status, task);
            }
        }

        private void InvokeCompletion(TaskStatus status)
        {
            if (Notify)
            {
                Completed?.Invoke(status);
            }
        }
        #endregion
    }
}
