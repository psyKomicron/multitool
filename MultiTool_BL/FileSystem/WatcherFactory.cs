using System.IO;

namespace Multitool.FileSystem
{
    internal static class WatcherFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="FileSystemWatcher"/> with the specified path and notify filters.
        /// </summary>
        /// <param name="path">Path for the watcher to watch</param>
        /// <param name="filters"><see cref="NotifyFilters"/></param>
        /// <param name="delegates">Delegates for <see cref="FileSystemWatcher"/> events. (see <see cref="WatcherDelegates"/>)</param>
        /// <returns>The created watcher</returns>
        public static FileSystemWatcher CreateWatcher(string path, NotifyFilters filters, WatcherDelegates delegates)
        {
            FileSystemWatcher watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = filters
            };

            watcher.Deleted += delegates.DeletedHandler;
            watcher.Created += delegates.CreatedHandler;
            watcher.Changed += delegates.ChangedHandler;
            watcher.Renamed += delegates.RenamedHandler;
            return watcher;
        }

        /// <summary>
        /// Creates an instance of <see cref="FileSystemWatcher"/> with the specified path.
        /// </summary>
        /// <param name="path">Path for the watcher to watch</param>
        /// <param name="delegates">Delegates for <see cref="FileSystemWatcher"/> events. (see <see cref="WatcherDelegates"/>)</param>
        /// <returns>The created watcher</returns>
        public static FileSystemWatcher CreateWatcher(string path, WatcherDelegates delegates)
        {
            NotifyFilters all = NotifyFilters.FileName
                 | NotifyFilters.DirectoryName
                 | NotifyFilters.Attributes
                 | NotifyFilters.Size
                 | NotifyFilters.LastWrite
                 | NotifyFilters.CreationTime
                 | NotifyFilters.Security;

            return CreateWatcher(path, all, delegates);
        }
    }
}
