using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Multitool.FileSystem
{
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
                        string subPath = subDirs[i];
                        Task<long> t = new Task<long>(() => CalculateDirectorySize(subPath, cancellationToken), cancellationToken);
                        dirTasks.Add(t);
                        t.Start();
                    }

                    Task<long[]> awaitable = Task.WhenAll(dirTasks);
                    dirTasks.Clear();
                    try
                    {
                        awaitable.Wait(cancellationToken);

                        for (int i = 0; i < awaitable.Result.Length; i++)
                        {
                            size += awaitable.Result[i];
                        }
                    }
                    catch (OperationCanceledException opCanceled)
                    {
                        opCanceled.Data.Add(GetType(), "Operation cancelled while waiting for children threads (calculating " + path +")");
                        throw;
                    }
                    catch (AggregateException aggregate)
                    {
                        for (int i = 0; i < aggregate.InnerExceptions.Count; i++)
                        {
                            if (aggregate.InnerExceptions[i].GetType() == typeof(OperationCanceledException))
                            {
                                aggregate.InnerExceptions[i].Data.Add(GetType(), "Operation cancelled while waiting for children threads (calculating " + path + ")");
                                throw aggregate.InnerExceptions[i];
                            }
                        }

                        throw;
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
            return size;
        }

        public long CalculateDirectorySize(string path, CancellationToken cancellationToken)
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
