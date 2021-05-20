using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Multitool.FileSystem
{
    public abstract class FileSystemEntry : IFileSystemEntry
    {
        private string _path;
        private string _name;

        /// <summary>Constructor.</summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        protected FileSystemEntry(FileSystemInfo info)
        {
            _path = info.FullName;
            _name = info.Name;
        }

        public abstract long Size { get; set; }
        public abstract FileSystemInfo Info { get; }

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
        public string Path
        {
            get => _path;
            set 
            {
                _path = value;
                NotifyPropertyChanged();
            }
        }
        /// <inheritdoc/>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }

        internal FileInfo FileInfo { get; set; }
        internal DirectoryInfo DirectoryInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Refreshes the internal <see cref="FileSystemInfo"/>.
        /// </summary>
        public void RefreshInfos()
        {
            Info.Refresh();
            SetInfos();
        }

        /// <inheritdoc/>
        public virtual void Delete()
        {
            if (CanDelete())
            {
                Info.Delete();
            }
            else
            {
                throw CreateIOException();
            }
        }

        /// <inheritdoc/>
        public void Rename(string newName)
        {
            if (IsDevice)
            {
                throw new IOException("Cannot delete a file with IsDevice tag");
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
        public void Move(string newPath)
        {
            if (File.Exists(newPath))
            {
                if (IsSystem)
                {
                    throw new IOException("Cannot move system file");
                }
                else if (DirectoryInfo != null)
                {
                    DirectoryInfo.MoveTo(newPath);
                }
                else if (FileInfo != null)
                {
                    FileInfo.MoveTo(newPath);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Directory (" + newPath + ") does not exist. File cannot be moved");
            }
        }

        /// <inheritdoc/>
        public void CopyTo(string newPath)
        {
            if (File.Exists(newPath))
            {
                if (IsSystem)
                {
                    throw new IOException("Cannot move system file");
                }
                else if (DirectoryInfo != null)
                {
                    DirectoryCopyTo(DirectoryInfo, newPath);
                }
                else if (FileInfo != null)
                {
                    FileInfo.CopyTo(newPath);
                }
            }
            else
            {
                throw new DirectoryNotFoundException("Directory (" + newPath + ") does not exist. File cannot be moved");
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
            else if (!IsDirectory && other.IsDirectory)
            {
                return 1;
            }

            if (Size > other.Size)
            {
                return -1;
            }
            else if (Size < other.Size)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        /// <inheritdoc/>
        public bool Equals(IFileSystemEntry other)
        {
            return Path.Equals(other.Path);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Set path and name of this <see cref="FileSystemEntry"/>. Use after refreshing info.
        /// </summary>
        protected void SetInfos()
        {
            Path = Info.FullName;
            Name = Info.Name;
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

        protected virtual bool CanDelete() => CanDelete(Info);

        protected IOException CreateIOException(FileSystemInfo info)
        {
            IOException e = new IOException("Cannot delete " + info.FullName);
            e.Data.Add(info.ToString(), info);
            return e;
        }

        protected IOException CreateIOException() => CreateIOException(Info);

        protected void RemoveReadOnly(FileSystemInfo info)
        {
            info.Attributes &= ~FileAttributes.ReadOnly;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void DirectoryCopyTo(DirectoryInfo info, string newPath)
        {
            DirectoryInfo[] dirs = info.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            DirectoryInfo newDir = Directory.CreateDirectory(newPath);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = info.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = System.IO.Path.Combine(newDir.FullName, file.Name); 
                file.CopyTo(tempPath, false);
            }

            // copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = System.IO.Path.Combine(newDir.FullName, subdir.Name);
                DirectoryCopyTo(subdir, tempPath);
            }
        }
    }
}
