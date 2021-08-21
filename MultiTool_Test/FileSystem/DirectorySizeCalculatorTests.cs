using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.FileSystem;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MultitoolTest.FileSystem.Tests
{
    [TestClass()]
    public class DirectorySizeCalculatorTests
    {
        [TestMethod()]
        public void DirectorySizeCalculatorTest()
        {
            DirectorySizeCalculator o = new DirectorySizeCalculator();
        }

        [TestMethod()]
        public void DirectorySizeCalculatorTest1()
        {
            var o = new DirectorySizeCalculator(true);
            Assert.IsTrue(o.Notify);
        }

        [TestMethod()]
        public void AsyncCalculateDirectorySizeTest()
        {
            Entry entry = new(new DirectoryEntry(new DirectoryInfo(@"c:\users")));
            DirectorySizeCalculator o = new();
            o.CalculateDirectorySizeAsync(@"c:\users", CancellationToken.None, entry.Size);
        }
    }

    class Entry : IFileSystemEntry
    {
        private IFileSystemEntry entry;

        public Entry(IFileSystemEntry fileEntry)
        {
            this.entry = fileEntry;
            fileEntry.SizedChanged += FileEntry_SizedChanged;
        }

        private void FileEntry_SizedChanged(IFileSystemEntry sender, long oldSize)
        {
            Console.WriteLine("Size updated: " + entry.Size + " (old size " + oldSize + ")");
        }

        #region decoration
        public FileAttributes Attributes => entry.Attributes;

        public FileSystemInfo Info => entry.Info;

        public bool IsCompressed => entry.IsCompressed;

        public bool IsDevice => entry.IsDevice;

        public bool IsDirectory => entry.IsDirectory;

        public bool IsEncrypted => entry.IsEncrypted;

        public bool IsHidden => entry.IsHidden;

        public bool IsReadOnly => entry.IsReadOnly;

        public bool IsSystem => entry.IsSystem;

        public string Name => entry.Name;

        public bool Partial => entry.Partial;

        public string Path => entry.Path;

        public long Size => entry.Size;

        public event EntryAttributesChangedEventHandler AttributesChanged
        {
            add
            {
                entry.AttributesChanged += value;
            }

            remove
            {
                entry.AttributesChanged -= value;
            }
        }

        public event EntryChangedEventHandler Deleted
        {
            add
            {
                entry.Deleted += value;
            }

            remove
            {
                entry.Deleted -= value;
            }
        }

        public event EntryRenamedEventHandler Renamed
        {
            add
            {
                entry.Renamed += value;
            }

            remove
            {
                entry.Renamed -= value;
            }
        }

        public event EntrySizeChangedEventHandler SizedChanged
        {
            add
            {
                entry.SizedChanged += value;
            }

            remove
            {
                entry.SizedChanged -= value;
            }
        }

        public int CompareTo(object obj)
        {
            return entry.CompareTo(obj);
        }

        public int CompareTo(IFileSystemEntry other)
        {
            return entry.CompareTo(other);
        }

        public void CopyTo(string newPath)
        {
            entry.CopyTo(newPath);
        }

        public void Delete()
        {
            entry.Delete();
        }

        public bool Equals(IFileSystemEntry other)
        {
            return entry.Equals(other);
        }

        public void Move(string newPath)
        {
            entry.Move(newPath);
        }

        public void Rename(string newName)
        {
            entry.Rename(newName);
        }
        #endregion
    }
}