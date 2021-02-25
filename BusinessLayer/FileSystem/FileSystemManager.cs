using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public void GetFiles(string path, CancellationToken cancellationToken, out List<PathItem> items)
        {
            if (!string.IsNullOrEmpty(path))
            {
                items = new List<PathItem>();

                if (Directory.Exists(path))
                {
                    try
                    {
                        string[] dirs = Directory.GetDirectories(path);

                        for (int i = 0; i < dirs.Length; i++)
                        {
                            CheckCancellation(cancellationToken);

                            long size = CancelableComputeDirectorySize(dirs[i], cancellationToken);

                            items.Add(new PathItem()
                            {
                                Path = dirs[i],
                                Size = size,
                                Name = new DirectoryInfo(dirs[i]).Name
                            });
                        }
                    }
                    catch (UnauthorizedAccessException) { }

                    try
                    {
                        string[] files = Directory.GetFiles(path);

                        for (int i = 0; i < files.Length; i++)
                        {
                            CheckCancellation(cancellationToken);

                            FileInfo fileInfo = new FileInfo(files[i]);

                            items.Add(new PathItem()
                            {
                                Path = files[i],
                                Size = fileInfo.Length,
                                Name = fileInfo.Name,
                                Attributes = fileInfo.Attributes
                            });
                        }
                    }
                    catch (UnauthorizedAccessException) { }
                }
            }
            else
            {
                items = null;
            }
        }

        /// <summary>
        /// Get files one by one thanks to the yield operator. 
        /// If preload attribute was set to true, method will try to get the files in the next and previous directory asynchronously.
        /// </summary>
        /// <param name="path">System file path</param>
        /// <returns>The file in the directory, encapsulated in a <see cref="PathItem"/></returns>
        public IEnumerable<PathItem> GetEnumeratorFiles(string path)
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

        private void CheckFileSystem(object changes)
        {
            // update previous & direct next files on system change

        }

        private long CancelableComputeDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    CheckCancellation(cancellationToken);

                    size += CancelableComputeDirectorySize(subDirPath, cancellationToken);
                }
            }
            catch (UnauthorizedAccessException) { }

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateFiles(path);

                foreach (string subDirPath in subDirPaths)
                {
                    CheckCancellation(cancellationToken);

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

        private void CheckCancellation(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        //#region events
        private void OnFileSystemChange(object sender, FileSystemEventArgs args)
        {

        }
        //#endregion
    }
}
