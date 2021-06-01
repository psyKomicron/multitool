using System.IO;

namespace Multitool.FileSystem
{
    internal class DirectoryEntry : FileSystemEntry
    {
        private long _size;

        public DirectoryEntry(DirectoryInfo info) : base(info)
        {
            DirectoryInfo = info;
        }

        public DirectoryEntry(DirectoryInfo info, long size) : base(info)
        {
            DirectoryInfo = info;
            _size = size;
        }

        public override long Size
        {
            get => _size;
            set
            {
                _size = value;
                NotifyPropertyChanged();
            }
        }

        public override void Delete()
        {
            if (CanDelete())
            {
                DeleteDir(Path);
            }
            else
            {
                throw CreateIOException();
            }
        }

        private void DeleteDir(string dirPath)
        {
            string[] dirs = Directory.GetDirectories(dirPath);
            for (int i = 0; i < dirs.Length; i++)
            {
                DeleteDir(dirs[i]);
            }

            FileInfo fileInfo;
            string[] files = Directory.GetFiles(dirPath);
            for (int i = 0; i < files.Length; i++)
            {
                fileInfo = new FileInfo(files[i]);
                if (CanDelete(fileInfo))
                {
                    fileInfo.Delete();
                }
                else
                {
                    throw CreateIOException(fileInfo);
                }
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            if (CanDelete(directoryInfo))
            {
                directoryInfo.Delete();
            }
            else
            {
                throw CreateIOException(directoryInfo);
            }
        }
    }
}
