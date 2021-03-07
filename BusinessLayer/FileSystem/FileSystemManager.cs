using BusinessLayer.FileSystem.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace BusinessLayer.FileSystem
{
    public class FileSystemManager
    {
        private static FileSystemManager instance;

        private double _ttl;
        private Dictionary<string, FileSystemCache> cache = new Dictionary<string, FileSystemCache>();
        private bool preload;

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

        protected FileSystemManager(bool preload, double ttl)
        {
            this.preload = preload;
            _ttl = ttl;
        }

        public static FileSystemManager Get(double ttl = 300000, bool preload = true)
        {
            if (instance == null)
            {
                instance = new FileSystemManager(preload, ttl);
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
        public void GetFiles<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, 
            CollectionAddDelegate<ItemType> addDelegate) where ItemType : IPathItem
        {
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
                    Console.Out.WriteLine("Getting files from cache (for path " + path + ") ...");
                    FileSystemCache cachedItems = GetFileSystemCache(path);

                    if (cachedItems.Partial)
                    {
                        GetPartial(path, cachedItems, list, addDelegate, cancellationToken);
                    }
                    else
                    {
                        for (int i = 0; i < cachedItems.Count; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            addDelegate(list, cachedItems[i]);
                        }
                    }
                }
                else if (Directory.Exists(path))
                {
                    FileSystemCache cacheItems = null;

                    try
                    {
                        cacheItems = BuildCache(path);
                    }
                    catch (FileNotFoundException fnf) { }

                    GetDirectories(path, cacheItems, list, addDelegate, cancellationToken);

                    GetFiles(path, cacheItems, list, addDelegate, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Empties the whole cache.
        /// </summary>
        public void ClearCache()
        {
            throw new NotImplementedException();
            cache.Clear();
        }

        /// <summary>
        /// Empties the <see cref="FileSystemCache"/> associated with the <paramref name="path"/> param.
        /// </summary>
        /// <param name="path"></param>
        public void ClearDirectoryCache(string path)
        {
            throw new NotImplementedException();
            if (!string.IsNullOrEmpty(path) && cache.ContainsKey(path))
            {
                cache.Remove(path);
            }
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
                FileInfo fileInfo = new FileInfo(path);
                DirectoryInfo root = fileInfo.Directory;
            }

            return realPath;
        }

        public void Refresh(string path)
        {
            if (!string.IsNullOrEmpty(path) && ContainsKey(path))
            {
                cache[path].Refresh();
            }
        }


        private void GetPartial<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IPathItem
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                string[] dirPaths = Directory.GetDirectories(path);
                string[] filePaths = Directory.GetFiles(path);
                List<string> toDo = new List<string>(dirPaths.Length);

                // compare the directories in the cache and the actual directories
                for (int i = 0; i < dirPaths.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string filePath = dirPaths[i];

                    for (int j = 0; j < cacheItems.Count; j++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        if (!cacheItems[j].Path.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                        {
                            toDo.Add(filePath);
                            break;
                        }
                    }
                }

                for (int i = 0; i < toDo.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();


                    if (File.Exists(path))
                    {
                        FileInfo fileInfo = new FileInfo(toDo[i]);
                        PathItem item = new PathItem(toDo[i], fileInfo.Length, fileInfo.Name, fileInfo.Attributes);

                        cacheItems?.Add(item);

                        addDelegate(list, item);
                    }
                    else
                    {
                        long size = DirectorySizeComputer.ComputeDirectorySize(toDo[i], cancellationToken);
                        DirectoryInfo info = new DirectoryInfo(toDo[i]);
                        PathItem item = new PathItem(toDo[i], size, info.Name, info.Attributes);

                        cacheItems.Add(item);

                        addDelegate(list, item);
                    }

                    
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        private void GetDirectories<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IPathItem
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
                        long size = DirectorySizeComputer.ComputeDirectorySize(dirPaths[i], cancellationToken);
                        DirectoryInfo info = new DirectoryInfo(dirPaths[i]);
                        PathItem item = new PathItem(dirPaths[i], size, info.Name, info.Attributes);

                        cacheItems?.Add(item);

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

        private void GetFiles<T>(string path, FileSystemCache cacheItems, IList<T> list, CollectionAddDelegate<T> addDelegate, CancellationToken cancellationToken) where T : IPathItem
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
                            cache.Remove(path); // remove the cached path since the op was cancelled
                        }

                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    FileInfo fileInfo = new FileInfo(files[i]);
                    PathItem item = new PathItem(files[i], fileInfo.Length, fileInfo.Name, fileInfo.Attributes);

                    cacheItems?.Add(item);

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

        private FileSystemCache BuildCache(string path)
        {
            FileSystemCache sysCache = new FileSystemCache(path, TTL);

            sysCache.ItemChanged += OnCacheItemChanged;
            sysCache.TTLReached += OnCacheTTLReached;

            cache.Add(path, sysCache);
            return sysCache;
        }

        private void RefreshCache(FileSystemCache cacheItems, string path)
        {
            cacheItems.Reset();
            if (Directory.Exists(path))
            {
                string[] dirs = null;
                try
                {
                    dirs = Directory.GetDirectories(path);
                }
                catch (UnauthorizedAccessException) { }

                if (dirs != null)
                {
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        long size = DirectorySizeComputer.ComputeDirectorySize(dirs[i]);
                        DirectoryInfo info = new DirectoryInfo(dirs[i]);
                        PathItem item = new PathItem(dirs[i], size, new DirectoryInfo(dirs[i]).Name, info.Attributes);
                        cacheItems.Add(item);
                    }
                }

                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (UnauthorizedAccessException) { }

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        FileInfo fileInfo = new FileInfo(files[i]);
                        PathItem item = new PathItem(files[i], fileInfo.Length, fileInfo.Name, fileInfo.Attributes);
                        cacheItems.Add(item);
                    }
                }
            }
        }

        private List<PathItem> GetAffectedItems(string path)
        {
            List<PathItem> itemsToChange = new List<PathItem>(50);
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
                            PathItem item = GetItemFromCache(parent.FullName, dir.FullName);
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

        private PathItem GetItemFromCache(string cachePath, string itemPath)
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
            if (e.FromTTL)
            {
                RefreshCache(e.Cache, e.Path);
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
            List<PathItem> itemsToChange = GetAffectedItems(path);
        }

        #endregion
    }
}
