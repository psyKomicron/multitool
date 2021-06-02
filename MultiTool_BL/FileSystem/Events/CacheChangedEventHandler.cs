using System;
using System.IO;

namespace Multitool.FileSystem.Events
{
    internal delegate void CacheChangedEventHandler(
        object sender, string name, FileSystemEntry entry, bool ttl, WatcherChangeTypes changes);
}
