using BusinessLayer.Events;
using BusinessLayer.FileSystem.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    public class FileSystemManager : IFileSystemManager
    {
        public const double DEFAULT_TTL = 300000;
        public const bool DEFAULT_NOTIFY_STATUS = false;

        private static FileSystemManager instance;

        private ObjectPool<>
        private double _ttl;
        private Stopwatch stopwatch = new Stopwatch();
        private Dictionary<string, FileSystemCache> cache = new Dictionary<string, FileSystemCache>();

        public event ProgressEventHandler Progress;

        public bool NotifyProgress { get; set; }

        public double TTL
        {
            get => _ttl;
            set
            {
                _ttl = value;
                foreach (KeyValuePair<string, FileSystemCache> c in cache)
                {
                    c.Value.UpdateTTL(value);
                }
            }
        }

        protected FileSystemManager() 
        {
            _ttl = DEFAULT_TTL;
            NotifyProgress = DEFAULT_NOTIFY_STATUS;
        }

        protected FileSystemManager(double ttl, bool notifyProgress)
        {
            _ttl = ttl;
            NotifyProgress = notifyProgress;
        }

        public static FileSystemManager Get(double ttl = DEFAULT_TTL, bool notifyProgress = DEFAULT_NOTIFY_STATUS)
        {
            if (instance == null)
            {
                instance = new FileSystemManager(ttl, notifyProgress);
            }
            return instance;
        }

        /// <summary>
        /// <para>
        /// List the content of a directory as a <see cref="IList{T}"/>. Because each directory size is calculated, the task can be 
        /// cancelled with the <paramref name="cancellationToken"/>. Please be aware that the <paramref name="list"/> needs to be 
        /// instanciated before calling this method and items will be directly added to it during method execution.
        /// </para>
        /// <para>
        /// The <see cref="CollectionAddDelegate{ItemType}"/> provide control on how the items are added to the collection.
        /// i.e. if the application UI is not multi-threaded.
        /// </para>
        /// </summary>
        /// <typeparam name="ItemType">Generic param of the <see cref="IList{T}"/></typeparam>
        /// <param name="path">System file path</param>
        /// <param name="cancellationToken">Cancellation token to cancel this method</param>
        /// <param name="list">Collection to add items to</param>
        /// <param name="addDelegate">Delegate to add items to the <paramref name="list"/></param>
        /// <exception cref="ArgumentNullException">
        /// If either <paramref name="list"/> or <paramref name="cancellationToken"/> is <see cref="null"/>
        /// </exception>
        public void GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list,
            CollectionAddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry
        {
            stopwatch.Start();

            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (cancellationToken == null)
            {
                throw new ArgumentNullException(nameof(cancellationToken));
            }

            if (!string.IsNullOrEmpty(path))
            {
                if (ContainsKey(path))
                {
                    Debug.WriteLine("Getting files from cache (for path " + path + ") ...");

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
                }
                else if (Directory.Exists(path))
                {
                    FileSystemCache cacheItems = null;

                    try
                    {
                        cacheItems = new FileSystemCache(path, TTL);

                        cacheItems.ItemChanged += OnCacheItemChanged;
                        cacheItems.TTLReached += OnCacheTTLReached;

                        cache.Add(path, cacheItems);
                    }
                    catch (FileNotFoundException) 
                    {
                        if (NotifyProgress) 
                        {
                            Progress?.Invoke(null, path + " not found..."); 
                        }
                    }

                    GetDirectories(path, cacheItems, list, addDelegate, cancellationToken);

                    GetFiles(path, cacheItems, list, addDelegate, cancellationToken);
                }
            }

            stopwatch.Stop();
            Progress?.Invoke(this, stopwatch.Elapsed.TotalSeconds.ToString());
            stopwatch.Reset();
        }

        /// <summary>
        /// Get the case sensitive path for the <paramref name="path"/> parameter.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetRealPath(string path)
        {
            string realPath = string.Empty;
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                List<DirectoryInfo> parents = new List<DirectoryInfo>(5);

                DirectoryInfo parent = directoryInfo.Parent;

                if (parent != null)
                {
                    IEnumerable<DirectoryInfo> directories = parent.EnumerateDirectories();
                    foreach (DirectoryInfo fileInfo in directories)
                    {
                        if (fileInfo.Name.Equals(directoryInfo.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            parents.Add(fileInfo);
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
                                    parents.Add(fileInfo);
                                }
                            }
                        }

                        parent = parent.Parent;
                    }
                    if (parent != null)
                    {
                        parents.Add(parent);
                    }

                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = parents.Count - 1; i >= 0; i--)
                    {
                        stringBuilder.Append(parents[i].Name);
                        if (!parents[i].Name.Contains(Path.DirectorySeparatorChar + "") && i != 0)
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
            else if (File.Exists(path))
            {
                /* TODO */
                FileInfo fileInfo = new FileInfo(path);
                DirectoryInfo root = fileInfo.Directory;
            }

            return realPath;
        }

        /// <summary>
        /// Cleans the internal cache.
        /// </summary>
        public void Reset()
        {
            foreach (KeyValuePair<string, FileSystemCache> pair in cache)
            {
                FileSystemCache cache = pair.Value;
                cache.Delete();
            }
            cache.Clear();
        }


        private void GetPartial<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] paths = Directory.GetFileSystemEntries(path);
                List<string> toDo = new List<string>(paths.Length - cacheItems.Count);

                // compare the file entries in the cache and the system  file entries
                for (int i = 0; i < paths.Length; i++)
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

                for (int i = 0; i < toDo.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (NotifyProgress)
                    {
                        Progress?.Invoke(null, "Working on " + toDo[i]);
                    }

                    if (File.Exists(toDo[i]))
                    {
                        FileInfo fileInfo = new FileInfo(toDo[i]);
                        FileSystemEntry item = new FileEntry(fileInfo);

                        cacheItems.Add(item);

                        addDelegate(list, item);
                    }
                    else if (Directory.Exists(toDo[i]))
                    {
                        long size = ComputeDirectorySize(toDo[i], cancellationToken);
                        DirectoryInfo info = new DirectoryInfo(toDo[i]);
                        FileSystemEntry item = new DirectoryEntry(info, size);

                        cacheItems.Add(item);

                        addDelegate(list, item);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private void GetDirectories<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] dirPaths = Directory.GetDirectories(path);

                for (int i = 0; i < dirPaths.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (cacheItems != null)
                        {
                            cacheItems.Partial = true; // set partial since the op was cancelled
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    try
                    {
                        if (NotifyProgress)
                        {
                            Progress?.Invoke(null, "Working on " + dirPaths[i]);
                        }

                        long size = ComputeDirectorySize(dirPaths[i], cancellationToken);
                        DirectoryInfo info = new DirectoryInfo(dirPaths[i]);
                        FileSystemEntry item = new DirectoryEntry(info, size);

                        cacheItems.Add(item);

                        addDelegate(list, item);
                    }
                    catch (OperationCanceledException e) // catch from ComputeDirectorySize() and set cache partial
                    {
                        if (cacheItems != null)
                        {
                            cacheItems.Partial = true;
                        }
                        throw e;
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private void GetFiles<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IFileSystemEntry
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        if (cacheItems != null)
                        {
                            cacheItems.Partial = true; // set partial since the op was cancelled
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    if (NotifyProgress)
                    {
                        Progress?.Invoke(null, "Working on " + files[i]);
                    }

                    FileInfo fileInfo = new FileInfo(files[i]);
                    FileSystemEntry item = new FileEntry(fileInfo);

                    cacheItems.Add(item);

                    addDelegate(list, item);
                }
            }
            catch (UnauthorizedAccessException) { }
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

        private long ComputeDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (NotifyProgress)
                    {
                        Progress?.Invoke(null, "Working on " + subDirPath);
                    }

                    size += ComputeDirectorySize(subDirPath, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException) { }

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateFiles(path);

                foreach (string subDirPath in subDirPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (NotifyProgress)
                    {
                        Progress?.Invoke(null, "Working on " + subDirPath);
                    }

                    try
                    {
                        size += new FileInfo(subDirPath).Length;
                    }
                    catch (FileNotFoundException) { }
                }
            }
            catch (UnauthorizedAccessException) { }

            return size;
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
        }

        private FileSystemEntry GetItemFromCache(string cachePath, string itemPath)
        {
            FileSystemCache systemCache = cache[cachePath];

            for (int i = 0; i < systemCache.Count; i++)
            {
                if (itemPath.Equals(systemCache[i].Path, StringComparison.OrdinalIgnoreCase))
                {
                    return systemCache[i];
                }
            }

            return null;
        }

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

                }
                else
                {

                }
            }
        }

        private void OnCacheItemChanged(object sender, FileSystemCacheEventArgs e)
        {
            if (e.ChangeTypes == WatcherChangeTypes.Renamed)
            {
                return;
            }

            string path = e.Path;
            FileSystemCache systemCache = sender as FileSystemCache;

            if (e.ItemChanged != null && !e.ItemChanged.IsDirectory)
            {
                List<FileSystemEntry> itemsToChange = GetAffectedItems(path);
            }
        }
        #endregion
    }
}
