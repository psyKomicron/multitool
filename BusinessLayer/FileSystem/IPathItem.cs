using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    public interface IPathItem : IComparable<IPathItem>, IComparable, INotifyPropertyChanged, IEquatable<IPathItem>
    {
        string Path { get; set; }

        long Size { get; set; }

        string Name { get; set; }

        FileAttributes Attributes { get; set; }

        bool IsHidden { get; }

        bool IsSystem { get; }

        bool IsReadOnly { get; }

        bool IsEncrypted { get; }

        bool IsCompressed { get; }

        bool IsDevice { get; }

        bool IsDirectory { get; }
    }
}
