using System.Collections.Generic;
using System.Threading;

namespace BusinessLayer.FileSystem
{
    public interface IFileSystemManager
    {
        double TTL { get; set; }

        void GetFiles<ItemType>(string path, CancellationToken cancellationToken, IList<ItemType> list, CollectionAddDelegate<ItemType> addDelegate) where ItemType : IPathItem;
        string GetRealPath(string path);
        void Reset();
    }
}