using Multitool.FileSystem.Events;
using Multitool.Optimisation;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Multitool.FileSystem
{
    public class FileSystemManager : IFileSystemManager
    {
        public const double DEFAULT_CACHE_TIMEOUT = 300000;
        public const bool   DEFAULT_NOTIFY_STATUS = false;

        private static FileSystemManager instance;

        private double ttl;
        private ObjectPool<ChangeEventArgs> objectPool = new ObjectPool<ChangeEventArgs>();
        private Dictionary<string, FileSystemCache> cache = new Dictionary<string, FileSystemCache>();
        private DirectorySizeCalculator calculator = new DirectorySizeCalculator();

        protected FileSystemManager()
        {
            ttl = DEFAULT_CACHE_TIMEOUT;
            Notify = DEFAULT_NOTIFY_STATUS;
        }

        protected FileSystemManager(double ttl, bool notifyProgress)
        {
            this.ttl = ttl;
            Notify = notifyProgress;
        }

        public bool Notify { get; set; }

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

        public event TaskProgressEventHandler Progress;
        public event TaskFailedEventHandler Exception;
        public event TaskCompletedEventHandler Completed;

        public event ItemChangedEventHandler Change;

        public static FileSystemManager Get(double cacheTimeout = DEFAULT_CACHE_TIMEOUT, bool notifyProgress = DEFAULT_NOTIFY_STATUS)
        {
            if (instance == null)
            {
                instance = new FileSystemManager(cacheTimeout, notifyProgress);
            }
            else
            {
                if (cacheTimeout != instance.CacheTimeout)
                {
                    instance.CacheTimeout = cacheTimeout;
                }
                if (notifyProgress != instance.Notify)
                {
                    instance.Notify = notifyProgress;
                }
            }
            return instance;
        }

        /// <inheritdoc/>
        public async Task GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken,
            IList<ItemType> list, AddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry
        {
            #region not null
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (cancellationToken == null)
                throw new ArgumentNullException(nameof(cancellationToken));
            #endregion

            if (!string.IsNullOrEmpty(path))
            {
                if (ContainsKey(path))
                {
                    FileSystemCache cachedItems = GetFileSystemCache(path);
                    for (int i = 0; i < cachedItems.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        addDelegate(list, cachedItems[i]);
                    }

                    if (cachedItems.Partial)
                    {
                        GetPartial(path, cachedItems, list, addDelegate, cancellationToken);
                    }

                    Completed?.Invoke(TaskStatus.RanToCompletion);
                }
                else if (Directory.Exists(path))
                {
                    FileSystemCache cacheItems = new FileSystemCache(path, CacheTimeout);
                    cacheItems.ItemChanged += OnCacheItemChanged;
                    cacheItems.TTLReached += OnCacheTTLReached;
                    cacheItems.WatcherError += OnWatcherError;
                    cache.Add(path, cacheItems);

                    GetFiles(path, cacheItems, list, addDelegate, cancellationToken);
                    await GetDirectories(path, cacheItems, list, addDelegate, cancellationToken);
                    Completed?.Invoke(TaskStatus.RanToCompletion);
                }
            }
            else
            {
                Completed?.Invoke(TaskStatus.Faulted);
                throw new ArgumentException("Given path is empty", nameof(path));
            }
        }

        /// <inheritdoc/>
        public string GetRealPath(string path)
        {
            string realPath = string.Empty;
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

        #region private

        #region file get

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
                    FileSystemEntry cacheItem = cacheItems[i];
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
                ParallelOptions parallelOptions = new ParallelOptions()
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = files.Length
                };
                Parallel.For(0, files.Length - 1, parallelOptions, (int i) =>
                {
                    CheckCancellation(cancellationToken, cacheItems);

                    InvokeProgress(files[i]);

                    FileSystemEntry item = new FileEntry(new FileInfo(files[i]));
                    cacheItems.Add(item);
                    addDelegate(list, item);
                });
            }
            catch (UnauthorizedAccessException e) 
            {
                InvokeException(e);
            }
        }

        private async Task GetDirectories<T>(string path, FileSystemCache cacheItems, IList<T> list, AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string[] dirPaths = Directory.GetDirectories(path);
                List<Task> tasks = new List<Task>(dirPaths.Length);
                for (int i = 0; i < dirPaths.Length; i++)
                {
                    CheckCancellation(cancellationToken, cacheItems);

                    InvokeProgress(dirPaths[i]);

                    string currentPath = dirPaths[i];
                    FileSystemEntry item = new DirectoryEntry(new DirectoryInfo(dirPaths[i]));
                    cacheItems.Add(item);
                    addDelegate(list, item);

                    tasks.Add(RunDirsParallel(cacheItems, item, currentPath, list, addDelegate, cancellationToken));
                }

                await Task.WhenAll(tasks);
                cacheItems.Partial = false;
            }
            catch (UnauthorizedAccessException e) 
            {
                InvokeException(e);
            }
        }

        #endregion

        private async Task RunDirsParallel<T>(
            FileSystemCache cacheItems, FileSystemEntry item, string currentPath, IList<T> list,
            AddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            CheckCancellation(cancellationToken, cacheItems);
            addDelegate(list, item);
            item.Size = await calculator.AsyncCalculateDirectorySize(currentPath, cancellationToken);
            item.Partial = false;
        }

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
            foreach (var pair in cache)
            {
                if (pair.Key.Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void CheckCancellation(CancellationToken token, FileSystemCache cacheItems)
        {
            if (token.IsCancellationRequested)
            {
                if (cacheItems != null)
                {
                    cacheItems.Partial = true; // set partial since the op was cancelled
                }
                token.ThrowIfCancellationRequested();
            }
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
            if (e.TTLUpdated) return;

            FileSystemCache fsCache = e.Cache;

            for (int i = 0; i < fsCache.Count; i++)
            {
                FileSystemEntry item = fsCache[i];
                item.RefreshInfos();

                if (item.IsDirectory)
                {
                    item.Size = calculator.CalculateDirectorySize(item.Path, CancellationToken.None);
                }
            }
        }

        private void OnCacheItemChanged(object sender, string path, FileSystemEntry entry, bool ttl, WatcherChangeTypes changes)
        {
            if (changes == WatcherChangeTypes.Created) // if entry is null => item created
            {
                if (Directory.Exists(path))
                {
                    long size = calculator.CalculateDirectorySize(path, CancellationToken.None);
                    entry = new DirectoryEntry(new DirectoryInfo(path), size);
                }
                else
                {
                    entry = new FileEntry(new FileInfo(path));
                }
            }
            ((FileSystemCache)sender).Add(entry);
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
                Exception?.Invoke(this, e);
            }
        }

        private void InvokeProgress(string message)
        {
            if (Notify)
            {
                Progress?.Invoke(this, message);
            }
        }
        #endregion
    }
}
