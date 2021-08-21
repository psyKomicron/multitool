using Multitool.Optimisation;

using System;
using System.IO;

namespace Multitool.FileSystem.Events
{
    /// <summary>
    /// Provides data for file system changes events.
    /// </summary>
    public class FileChangeEventArgs : EventArgs, IPoolableObject
    {
        private bool _inUse;

        public FileChangeEventArgs() : base()
        {
            Entry = null;
            ChangeTypes = WatcherChangeTypes.All;
            InUse = false;
        }

        public FileChangeEventArgs(FileSystemEntry entry, WatcherChangeTypes changeTypes) : base()
        {
            Entry = entry;
            ChangeTypes = changeTypes;
        }

        public IFileSystemEntry Entry { get; internal set; }
        public WatcherChangeTypes ChangeTypes { get; internal set; }
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
