using System;
using System.ComponentModel;
using System.IO;

namespace Multitool.FileSystem
{
    public interface IFileSystemEntry : IComparable, INotifyPropertyChanged, IEquatable<IFileSystemEntry>, IComparable<IFileSystemEntry>
    {
        string Path { get; }
        long Size { get; }
        string Name { get; }
        FileAttributes Attributes { get; }
        bool IsHidden { get; }
        bool IsSystem { get; }
        bool IsReadOnly { get; }
        bool IsEncrypted { get; }
        bool IsCompressed { get; }
        bool IsDevice { get; }
        bool IsDirectory { get; }
    }
}
