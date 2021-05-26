using Multitool.FileSystem.Events;

using System;
using System.IO;
using System.Threading;

namespace Multitool.FileSystem
{
    /// <summary>
    /// TODO : make an actual async method that updates the size in real time
    /// </summary>
    public class DirectorySizeCalculator : IProgressNotifier
    {
        public DirectorySizeCalculator() { }

        public DirectorySizeCalculator(bool notify)
        {
            Notify = notify;
        }

        public bool Notify { get; set; }

        public event TaskProgressEventHandler Progress;
        public event TaskFailedEventHandler Exception;
        public event TaskCompletedEventHandler Completed;

        public long AsyncCalculateDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string[] subDirs = Directory.GetDirectories(path);
                for (int i = 0; i < subDirs.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    InvokeProgressAsync(subDirs[i]);
                    size += AsyncCalculateDirectorySize(subDirs[i], cancellationToken);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                InvokeExceptionAsync(e);
            }
            catch (DirectoryNotFoundException de)
            {
                InvokeExceptionAsync(de);
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                string[] files = Directory.GetFiles(path);

                for (int i = 0; i < files.Length; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    InvokeProgressAsync(files[i]);
                    try
                    {
                        size += new FileInfo(files[i]).Length;
                    }
                    catch (FileNotFoundException e)
                    {
                        InvokeExceptionAsync(e);
                    }
                }
            }
            catch (UnauthorizedAccessException e)
            {
                InvokeExceptionAsync(e);
            }
            catch (FileNotFoundException fe)
            {
                InvokeExceptionAsync(fe);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return size;
        }

        public long SyncCalculateDirectorySize(string path)
        {
            long size = 0;

            try
            {
                string[] subDirPaths = Directory.GetDirectories(path);
                for (int i = 0; i < subDirPaths.Length; i++)
                {
                    size += SyncCalculateDirectorySize(subDirPaths[i]);
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }

            try
            {
                string[] subDirPaths = Directory.GetFiles(path);

                for (int i = 0; i < subDirPaths.Length; i++)
                {
                    try
                    {
                        size += new FileInfo(subDirPaths[i]).Length;
                    }
                    catch (FileNotFoundException) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (FileNotFoundException) { }

            return size;
        }

        private void InvokeExceptionAsync(Exception e)
        {
            if (Notify)
            {
                Exception?.BeginInvoke(this, e, null, null);
            }
        }

        private void InvokeProgressAsync(string message)
        {
            if (Notify)
            {
                Progress?.BeginInvoke(this, message, null, null);
            }
        }
    }
}
