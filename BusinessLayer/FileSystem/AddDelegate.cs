using System.Collections.Generic;

namespace BusinessLayer.FileSystem
{
    public delegate void AddDelegate<ItemType>(IList<ItemType> items, IFileSystemEntry item) where ItemType : IFileSystemEntry;
}
