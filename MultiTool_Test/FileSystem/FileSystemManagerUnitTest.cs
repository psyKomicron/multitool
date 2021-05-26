using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using Multitool.FileSystem;

namespace BusinessLayer.FileSystem.Tests
{
    [TestClass()]
    public class FileSystemManagerUnitTest
    {
        [TestMethod()]
        public void GetTest()
        {
            FileSystemManager manager1 = FileSystemManager.Get();
            FileSystemManager manager2 = FileSystemManager.Get();

            Assert.AreSame(manager1, manager2);

            manager2 = FileSystemManager.Get(100, true);

            Assert.AreSame(manager1, manager2, "Singleton not respected");
            Assert.IsTrue(manager1.Notify == true, "Notify property not updated");
            Assert.IsTrue(manager1.CacheTimeout == 100, "CacheTimeout property not updated");
        }

        [TestMethod()]
        public void GetFileSystemEntriesTest()
        {
            #region instanciations
            FileSystemManager manager = FileSystemManager.Get();
            CancellationToken cancellationToken = new();
            IList<IFileSystemEntry> entries = new List<IFileSystemEntry>();
            #endregion

            string path = @"C:\Users\julie\Documents";
            string[] items = Directory.GetFileSystemEntries(path);
            manager.GetFileSystemEntries(path, cancellationToken, entries, (IList<IFileSystemEntry> list, IFileSystemEntry entry) => { list.Add(entry); });

            Assert.AreEqual(items.Length, entries.Count);

            for (int i = 0; i < items.Length; i++)
            {
                string item = items[i];
                bool contains = false;
                for (int j = 0; j < entries.Count; j++)
                {
                    if (entries[j].Path == item)
                    {
                        contains = true;
                    }
                }

                Assert.IsTrue(contains, nameof(entries) + " does not contains " + item);
            }
        }

        [TestMethod()]
        public void GetRealPathTest()
        {
            FileSystemManager manager = FileSystemManager.Get();

            string realPath = manager.GetRealPath(@"c:\users\julie\documents");
            Assert.AreEqual(@"C:\Users\julie\Documents", realPath);

            //realPath = manager.GetRealPath(@"e:\julie\Videos\Nyotengu(April 10th, 2021)");
            //Assert.AreEqual(@"E:\julie\Videos\Nyotengu(April 10th, 2021)", realPath);
        }

        [TestMethod()]
        public void ResetTest()
        {
            FileSystemManager manager = FileSystemManager.Get();
            manager.Reset();
        }
    }
}