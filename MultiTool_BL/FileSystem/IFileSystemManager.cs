using System.Collections.Generic;
using System.Threading;

namespace Multitool.FileSystem
{
    public interface IFileSystemManager : IProgressNotifier
    {
        double CacheTimeout { get; set; }

        void GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken, ref IList<ItemType> list, AddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry;
        string GetRealPath(string path);
        void Reset();
    }
}