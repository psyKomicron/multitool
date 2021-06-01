using Multitool.FileSystem.Events;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<long> AsyncCalculateDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;
            try
            {
                await Task.Run(() =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        List<Task<long>> dirTasks = new List<Task<long>>();
                        string[] subDirs = Directory.GetDirectories(path);
                        for (int i = 0; i < subDirs.Length; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            InvokeProgressAsync(subDirs[i]);
                            Task<long> t = new Task<long>(() => CalculateDirectorySize(subDirs[i], cancellationToken), cancellationToken);
                            dirTasks.Add(t);
                            //t.Start();
                        }

                        try
                        {
                            Task.WaitAll(dirTasks.ToArray());


                            for (int i = 0; i < dirTasks.Count; i++)
                            {
                                size += dirTasks[i].Result;
                            }

                        }
                        catch (IndexOutOfRangeException e)
                        {
                            Console.WriteLine("multitoo_bl [AsyncCalculateDirectorySize - tasks] > " + e.GetType().Name + ", " + e.Message);
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
                }, cancellationToken);

            }
            catch (Exception e)
            {
                Console.WriteLine("multitoo_bl [AsyncCalculateDirectorySize] > " + e.GetType().Name + ", " + e.Message);
            }
            return size;
        }

        public long CalculateDirectorySize(string path, CancellationToken cancellationToken)
        {
            try
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
                        size += CalculateDirectorySize(subDirs[i], cancellationToken);
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
            catch (Exception e)
            {
                Console.WriteLine("multitoo_bl [CalculateDirectorySize] dirs > " + e.GetType().Name + ", " + e.Message);
            }
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
