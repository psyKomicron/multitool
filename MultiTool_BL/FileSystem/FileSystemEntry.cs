
using Multitool.FileSystem.Events;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Multitool.FileSystem
{
    /// <summary>
    /// Base class for directory and file entries
    /// </summary>
    public abstract class FileSystemEntry : IFileSystemEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="info"></param>
        protected FileSystemEntry(FileSystemInfo info)
        {
            Path = info.FullName;
            Name = info.Name;
            Partial = true;
            Info = info;
        }

        #region properties
        /// <inheritdoc/>
        public abstract long Size { get; set; }

        /// <inheritdoc/>
        public FileSystemInfo Info { get; protected set; }
        /// <inheritdoc/>
        public FileAttributes Attributes => Info.Attributes;
        /// <inheritdoc/>
        public bool IsHidden => (Attributes & FileAttributes.Hidden) != 0;
        /// <inheritdoc/>
        public bool IsSystem => (Attributes & FileAttributes.System) != 0;
        /// <inheritdoc/>
        public bool IsReadOnly => (Attributes & FileAttributes.ReadOnly) != 0;
        /// <inheritdoc/>
        public bool IsEncrypted => (Attributes & FileAttributes.Encrypted) != 0;
        /// <inheritdoc/>
        public bool IsCompressed => (Attributes & FileAttributes.Compressed) != 0;
        /// <inheritdoc/>
        public bool IsDevice => (Attributes & FileAttributes.Device) != 0;
        /// <inheritdoc/>
        public bool IsDirectory => (Attributes & FileAttributes.Directory) != 0;
        /// <inheritdoc/>
        public string Path { get; set; }
        /// <inheritdoc/>
        public string Name { get; set; }
        /// <inheritdoc/>
        public bool Partial { get; set; }
        #endregion

        #region events

        /// <inheritdoc/>
        public event EntryChangedEventHandler Deleted;
        /// <inheritdoc/>
        public event EntrySizeChangedEventHandler SizedChanged;
        /// <inheritdoc/>
        public event EntryAttributesChangedEventHandler AttributesChanged;
        /// <inheritdoc/>
        public event EntryRenamedEventHandler Renamed;

        #endregion

        #region abstract methods
        public abstract void CopyTo(string newPath);
        /// <inheritdoc/>
        public abstract void Move(string newPath);
        /// <inheritdoc/>
        public abstract void RefreshInfos();
        #endregion

        #region public methods
        /// <summary>
        /// Refreshes the internal <see cref="FileSystemInfo"/>.
        /// </summary>

        /// <inheritdoc/>
        public virtual void Delete()
        {
            if (CanDelete())
            {
                Info.Delete();
            }
            else
            {
                throw CreateDeleteIOException();
            }
        }

        /// <inheritdoc/>
        public void Rename(string newName)
        {
            if (IsDevice)
            {
                throw new IOException("Cannot delete a file with device tag");
            }
            else if (IsSystem)
            {
                throw new IOException("Cannot delete a system file");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            if (obj is FileSystemEntry that)
            {
                return CompareTo(that);
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public int CompareTo(IFileSystemEntry other)
        {
            if (IsDirectory && !other.IsDirectory)
            {
                return -1;
            }
            if (!IsDirectory && other.IsDirectory)
            {
                return 1;
            }

            if (Size > other.Size)
            {
                return -1;
            }
            if (Size < other.Size)
            {
                return 1;
            }
            return 0;
        }

        /// <inheritdoc/>
        public bool Equals(IFileSystemEntry other)
        {
            return Path.Equals(other.Path);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name + ", " + Path;
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Set path and name of this <see cref="FileSystemEntry"/>. Use after refreshing info.
        /// </summary>
        protected void SetInfos(FileSystemInfo newInfo)
        {
            Info = newInfo;
            Path = Info.FullName;
            Name = Info.Name;
        }

        protected virtual bool CanMove(string newPath, out MoveCodes res)
        {
            if (File.Exists(newPath))
            {
                if (IsSystem)
                {
                    res = MoveCodes.IsSystem;
                    return false;
                }
                else if (Info == null)
                {
                    res = MoveCodes.InfoNotSet;
                    return false;
                }
                else
                {
                    res = MoveCodes.Possible;
                    return false;
                }
            }
            else
            {
                res = MoveCodes.PathNotFound;
                return false;
            }
        }

        protected virtual bool CanDelete(FileSystemInfo fileInfo)
        {
            if (((fileInfo.Attributes & FileAttributes.Device) != 0) || ((fileInfo.Attributes & FileAttributes.System) != 0))
            {
                if (fileInfo.Name == "desktop.ini")
                {
                    if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
                    {
                        RemoveReadOnly(fileInfo);
                    }
                    return true;
                }

                return false;
            }

            if ((fileInfo.Attributes & FileAttributes.ReadOnly) != 0)
            {
                RemoveReadOnly(fileInfo);
            }
            return true;
        }

        protected virtual bool CanDelete()
        {
            return CanDelete(Info);
        }

        protected IOException CreateDeleteIOException(FileSystemInfo info)
        {
            IOException e = new IOException("Cannot delete " + info.FullName);
            e.Data.Add(info.ToString(), info);
            return e;
        }

        protected IOException CreateDeleteIOException()
        {
            return CreateDeleteIOException(Info);
        }

        protected void RemoveReadOnly(FileSystemInfo info)
        {
            info.Attributes &= ~FileAttributes.ReadOnly;
            AttributesChanged?.Invoke(this, FileAttributes.ReadOnly);
        }

        protected void RaiseDeletedEvent()
        {
            Deleted?.Invoke(this, new FileChangeEventArgs(this, WatcherChangeTypes.Deleted));
        }

        protected void RaiseSizeChangedEvent(long oldSize)
        {
            SizedChanged?.Invoke(this, oldSize);
        }

        protected void RaiseAttributesChangedEvent(FileAttributes attributes)
        {
            AttributesChanged?.Invoke(this, attributes);
        }

        protected void RaiseRenamedEvent(string oldPath)
        {
            Renamed?.Invoke(this, oldPath);
        }
        #endregion

        #region events

        #region watcher events
        private void OnFileChange(object sender, FileSystemEventArgs e)
        {
            
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            RaiseDeletedEvent();
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            Path = e.FullPath;
            Name = e.Name;
            RaiseRenamedEvent(e.OldFullPath);
        }
        #endregion

        #endregion
    }
}
