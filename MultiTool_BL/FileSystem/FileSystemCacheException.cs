using System;

namespace Multitool.FileSystem
{
    internal class FileSystemCacheException : Exception
    {
        public FileSystemCacheException(string message): base(message)
        {
        }

        public FileSystemCacheException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
