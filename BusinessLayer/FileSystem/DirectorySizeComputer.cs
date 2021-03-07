using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BusinessLayer.FileSystem
{
    internal static class DirectorySizeComputer
    {
        public static long ComputeDirectorySize(string path, CancellationToken cancellationToken)
        {
            long size = 0;

            try
            {
                IEnumerable<string> subDirPaths = Directory.EnumerateDirectories(path);
                foreach (string subDirPath in subDirPaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

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
    }
}
