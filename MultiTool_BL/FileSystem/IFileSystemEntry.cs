using Multitool.FileSystem.Events;

using System;
using System.IO;

namespace Multitool.FileSystem
{
    public delegate void EntryChangedEventHandler(IFileSystemEntry sender, FileChangeEventArgs e);
    public delegate void EntrySizeChangedEventHandler(IFileSystemEntry sender, long oldSize);
    public delegate void EntryAttributesChangedEventHandler(IFileSystemEntry sender, FileAttributes attributeChanged);
    public delegate void EntryRenamedEventHandler(IFileSystemEntry sender, string oldPath);

    /// <summary>
    /// Defines a <see cref="FileSystemInfo"/> decorator
    /// </summary>
    public interface IFileSystemEntry : IComparable, IComparable<IFileSystemEntry>, IEquatable<IFileSystemEntry>
    {
        #region properties

        /// <summary>
        /// Attributes
        /// </summary>
        FileAttributes Attributes { get; }

        /// <summary>
        /// The underlying <see cref="FileSystemInfo"/> decorated by <see cref="IFileSystemEntry"/>
        /// </summary>
        FileSystemInfo Info { get; }

        /// <summary>
        /// True if the file is compressed
        /// </summary>
        bool IsCompressed { get; }

        /// <summary>
        /// True is the file is considered device
        /// </summary>
        bool IsDevice { get; }

        /// <summary>
        /// True if the file is a directory
        /// </summary>
        bool IsDirectory { get; }

        /// <summary>
        /// True if the file is encrypted
        /// </summary>
        bool IsEncrypted { get; }

        /// <summary>
        /// True if the file is hidden
        /// </summary>
        bool IsHidden { get; }

        /// <summary>
        /// True if the file is readonly
        /// </summary>
        bool IsReadOnly { get; }

        /// <summary>
        /// True if the file belongs to the system
        /// </summary>
        bool IsSystem { get; }

        /// <summary>
        /// Name of the file (not the path)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if the entry is marked as partial, meaning that this entry has not been fully computed yet.
        /// </summary>
        bool Partial { get; }

        /// <summary>
        /// Path of the file (system full path)
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Size of the entry on the disk
        /// </summary>
        long Size { get; }
        #endregion

        #region events
        /// <summary>
        /// Raises when the attributes are changed (IsCompressed, IsDevice, IsDirectory, ...);
        /// </summary>
        event EntryAttributesChangedEventHandler AttributesChanged;

        /// <summary>
        /// Raised when the entry no longer exists on the disk (has been moved or deleted)
        /// </summary>
        event EntryChangedEventHandler Deleted;

        /// <summary>
        /// Raised when renamed
        /// </summary>
        event EntryRenamedEventHandler Renamed;

        /// <summary>
        /// Raised when the size changes 
        /// </summary>
        event EntrySizeChangedEventHandler SizedChanged;
        #endregion

        #region methods
        /// <summary>
        /// Deletes the file
        /// </summary>
        void Delete();

        /// <summary>
        /// Rename the file
        /// </summary>
        /// <param name="newName">The new name of the file</param>
        void Rename(string newName);

        /// <summary>
        /// Move the file to a new directory
        /// </summary>
        /// <param name="newPath">The path to move the file to</param>
        /// <exception cref="IOException">Thrown when the entry cannot be moved.</exception>
        void Move(string newPath);

        /// <summary>
        /// Copy the file to a new directory
        /// </summary>
        /// <param name="newPath">The path to copy the file to</param>
        void CopyTo(string newPath);
        #endregion
    }
}
