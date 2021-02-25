using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    internal class FileSystemCacheException : Exception
    {
        public FileSystemCacheException(string message): base(message)
        {
        }
    }
}
