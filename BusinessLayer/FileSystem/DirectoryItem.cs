using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    internal class DirectoryItem : PathItem
    {
        private DirectoryInfo _info;
        private long _size;

        public override long Size
        {
            get => _size;
            set
            {
                _size = value;
                NotifyPropertyChanged();
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

        public DirectoryItem(DirectoryInfo info, long size)
        {
            _info = info;
            _size = size;
        }

        public override void RefreshInfos()
        {
            _info.Refresh();
            SetInfo(_info);
        }

        protected void SetInfo(DirectoryInfo info)
        {
            this._info = info;
            Path = info.FullName;
            Name = info.Name;
        }
    }
}
