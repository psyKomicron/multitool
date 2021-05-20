using System;

namespace Multitool.FileSystem.Events
{
    internal delegate void WatcherErrorEventHandler(FileSystemCache sender, Exception e, WatcherErrorTypes errType);
}
