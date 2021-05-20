using Multitool.Optimisation;

using System.IO;

namespace Multitool.FileSystem.Events
{
    public class ChangeEventArgs : IPoolableObject
    {
        private bool _inUse;

        public ChangeEventArgs()
        {
            Entry = null;
            ChangeTypes = WatcherChangeTypes.All;
            Parent = null;
            InUse = false;
        }

        public ChangeEventArgs(FileSystemEntry entry, WatcherChangeTypes changeTypes)
        {
            Entry = entry;
            if (entry.IsDirectory)
            {
                Parent = entry.DirectoryInfo.Parent;
            }
            else
            {
                Parent = entry.FileInfo.Directory;
            }

            ChangeTypes = changeTypes;
        }

        public IFileSystemEntry Entry { get; internal set; }
        public WatcherChangeTypes ChangeTypes { get; internal set; }
        public DirectoryInfo Parent { get; private set; }
        /// <inheritdoc/>
        public bool InUse
        {
            get => _inUse;
            set
            {
                _inUse = value;
                if (!_inUse)
                {
                    Free?.Invoke(this);
                }
            }
        }

        /// <inheritdoc/>
        public event FreeObjectEventHandler Free;
    }
}
