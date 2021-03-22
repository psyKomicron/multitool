using System.Collections.Generic;
using System.Threading;

namespace BusinessLayer.FileSystem
{
    public interface IFileSystemManager : IProgressNotifier
    {
        double TTL { get; set; }

        void GetFileSystemEntries<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, CollectionAddDelegate<ItemType> addDelegate) where ItemType : IFileSystemEntry;
        string GetRealPath(string path);
        void Reset();
    }
}