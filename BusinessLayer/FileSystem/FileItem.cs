using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    internal class FileItem : PathItem
    {
        private FileInfo _info;

        public override long Size
        {
            get =>_info.Length;
            set
            {
                throw new InvalidOperationException("Cannot set the size of a file, property is reliant on the actual file system infos.");
            }
        }
        public override FileAttributes Attributes => _info.Attributes;
        public override bool IsHidden => (_info.Attributes & FileAttributes.Hidden) != 0;
        public override bool IsSystem => (_info.Attributes & FileAttributes.System) != 0;
        public override bool IsReadOnly => (_info.Attributes & FileAttributes.ReadOnly) != 0;
        public override bool IsEncrypted => _info.Attributes.HasFlag(FileAttributes.Encrypted);
        public override bool IsCompressed => (_info.Attributes & FileAttributes.Compressed) != 0;
        public override bool IsDevice => (_info.Attributes & FileAttributes.Device) != 0;
        public override bool IsDirectory => (_info.Attributes & FileAttributes.Directory) != 0;

        public override void RefreshInfos()
        {
            _info.Refresh();
        }
    }
}
