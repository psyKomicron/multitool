using System;
using System.ComponentModel;
using System.IO;

namespace Multitool.FileSystem
{
    public interface IFileSystemEntry : IComparable, INotifyPropertyChanged, IEquatable<IFileSystemEntry>, IComparable<IFileSystemEntry>
    {
        FileAttributes Attributes { get; }
        FileSystemInfo Info { get; }
        bool IsCompressed { get; }
        bool IsDevice { get; }
        bool IsDirectory { get; }
        bool IsEncrypted { get; }
        bool IsHidden { get; }
        bool IsReadOnly { get; }
        bool IsSystem { get; }
        string Name { get; }
        bool Partial { get; }
        string Path { get; }
        long Size { get; }

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
        void Move(string newPath);

        /// <summary>
        /// Copy the file to a new directory
        /// </summary>
        /// <param name="newPath">The path to copy the file to</param>
        void CopyTo(string newPath);
    }
}
