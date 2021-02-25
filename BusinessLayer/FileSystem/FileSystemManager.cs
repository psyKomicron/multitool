using System;
using System.Collections.Generic;
using System.IO;
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

        protected FileSystemManager(bool preload)
        {
            this.preload = preload;
            _ttl = 300000;
        }

        public static FileSystemManager Get(bool preload = true)
        {
            if (instance == null)
            {
                instance = new FileSystemManager(preload);
            }

            return instance;
        }

        public bool IsPreloading { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ItemType">Generic param of the <see cref="IList{T}"/></typeparam>
        /// <param name="path">System file path</param>
        /// <param name="cancellationToken">Cancellation token to cancel this method</param>
        /// <param name="list">Collection to add items to</param>
        /// <param name="addDelegate">Delegate to add items to the <paramref name="list"/></param>
        /// <exception cref="ArgumentNullException">If either <paramref name="list"/> or <paramref name="cancellationToken"/> is <see cref="null"/></exception>
        public void GetFiles<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, CollectionAddDelegate<ItemType> addDelegate) where ItemType : IPathItem
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
                if (cache.ContainsKey(path))
                {
                    FileSystemCache cachedItems = cache[path];

                    for (int i = 0; i < cachedItems.Count; i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        addDelegate(list, cachedItems[i]);
                    }
                }
                else if (Directory.Exists(path))
                {
                    FileSystemCache cacheItems = BuildCache(path);

                    try
                    {
                        string[] dirs = Directory.GetDirectories(path);

                        for (int i = 0; i < dirs.Length; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cache.Remove(path); // remove the cached path since the op was cancelled
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            try
                            {
                                long size = CancelableComputeDirectorySize(dirs[i], cancellationToken);

                                DirectoryInfo info = new DirectoryInfo(dirs[i]);

                                PathItem item = new PathItem()
                                {
                                    Path = dirs[i],
                                    Size = size,
                                    Name = new DirectoryInfo(dirs[i]).Name,
                                    Attributes = info.Attributes
                                };

                                cacheItems.Add(item);

                                addDelegate(list, item);
                            }
                            catch (OperationCanceledException e)
                            {
                                cache.Remove(path);
                                throw e;
                            }
                        }
                    }
                    catch (UnauthorizedAccessException) { }

                    try
                    {
                        string[] files = Directory.GetFiles(path);

                        for (int i = 0; i < files.Length; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cache.Remove(path); // remove the cached path since the op was cancelled
                                cancellationToken.ThrowIfCancellationRequested();
                            }

                            FileInfo fileInfo = new FileInfo(files[i]);

                            PathItem item = new PathItem()
                            {
                                Path = files[i],
                                Size = fileInfo.Length,
                                Name = fileInfo.Name,
                                Attributes = fileInfo.Attributes
                            };

                            cacheItems.Add(item);

                            addDelegate(list, item);
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
        }

        /// <summary>
        /// Get files one by one thanks to the yield operator. 
        /// If preload attribute was set to true, method will try to get the files in the next and previous directory asynchronously.
        /// </summary>
        /// <param name="path">System file path</param>
        /// <returns>The file in the directory, encapsulated in a <see cref="PathItem"/></returns>
        public IEnumerable<PathItem> GetFiles(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (cache.ContainsKey(path))
                {
                    FileSystemCache items = cache[path];

                    for (int i = 0; i < items.Count; i++)
                    {
                        yield return items[i];
                    }
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        // build cache
                        FileSystemCache cacheItems = BuildCache(path);

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
                                long size = ComputeDirectorySize(dirs[i]);
                                DirectoryInfo info = new DirectoryInfo(dirs[i]);

                                PathItem item = new PathItem()
                                {
                                    Path = dirs[i],
                                    Size = size,
                                    Name = new DirectoryInfo(dirs[i]).Name,
                                    Attributes = info.Attributes
                                };

                                cacheItems.Add(item);
                                
                                yield return item;
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

                                PathItem item = new PathItem()
                                {
                                    Path = files[i],
                                    Size = fileInfo.Length,
                                    Name = fileInfo.Name,
                                    Attributes = fileInfo.Attributes
                                };

                                cacheItems.Add(item);

                                yield return item;
                            }
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        public void ClearCache()
        {
            cache.Clear();
        }

        public void ClearDirectoryCache(string path)
        {
            if (!string.IsNullOrEmpty(path) && cache.ContainsKey(path))
            {
                cache.Remove(path);
            }
        }

        public static long ComputeDirectorySize(string path)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    size += ComputeDirectorySize(subDirPath);
                }
            }
            catch (UnauthorizedAccessException) { }

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateFiles(path);

                foreach (string subDirPath in subDirPaths)
                {
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

        private FileSystemCache BuildCache(string path)
        {
            FileSystemCache sysCache = new FileSystemCache(path, TTL);
            cache.Add(path, sysCache);
            return sysCache;
        }

        private long CancelableComputeDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    size += CancelableComputeDirectorySize(subDirPath, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException) { }

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateFiles(path);

                foreach (string subDirPath in subDirPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

        #region events
        private void OnFileSystemChange(object sender, FileSystemEventArgs args)
        {

        }

        private void CheckFileSystem(object changes)
        {
            // update previous & direct next files on system change

        }
        #endregion
    }
}
