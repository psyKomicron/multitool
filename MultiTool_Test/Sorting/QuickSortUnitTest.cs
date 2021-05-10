using Microsoft.VisualStudio.TestTools.UnitTesting;
using Multitool.FileSystem;
using Multitool.Sorting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace BusinessLayer.Tests
{
    [TestClass]
    public class QuickSortUnitTest
    {
        [TestMethod]
        public void PartitionTest()
        {
            int[] array = { 10, 80, 30, 90, 40, 50, 70 };
            QuickSort.Partition(array, 0, array.Length - 1);
            Assert.AreEqual(70, array[4]);
        }

        [TestMethod]
        void SortTest<T>() where T : IComparable<T>
        {
            SortTestIntArray_1();
            SortTestIntArray_2();
            SortTestFSEArray();
        }

        [TestMethod]
        public void SortTestIntArray_1()
        {
            int[] intArray = { 10, 80, 30, 90, 40, 50, 70 };

            QuickSort.Sort(intArray, 0, intArray.Length - 1);
            for (int i = 0; i < intArray.Length - 1; i++)
            {
                Assert.IsFalse(intArray[i].CompareTo(intArray[i + 1]) > 0, nameof(intArray) + " is not sorted.");
            }
        }

        [TestMethod]
        public void SortTestIntArray_2()
        {
            int[] intArray2 = { 10, 7, 8, 9, 1, 5 };
            QuickSort.Sort(intArray2, 0, intArray2.Length - 1);
            for (int i = 0; i < intArray2.Length - 1; i++)
            {
                Assert.IsFalse(intArray2[i].CompareTo(intArray2[i + 1]) > 0, nameof(intArray2) + " is not sorted.");
            }
        }

        [TestMethod]
        public void SortTestFSEArray()
        {
            string[] paths = Directory.GetFileSystemEntries(@"C:\Users\julie\Documents\MultiTool\tests");
            var array = new FileSystemEntry[10];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new FileEntry(new FileInfo(paths[i]));
            }

            QuickSort.Sort(array, 0, array.Length - 1);

            for (int i = 0; i < array.Length - 1; i++)
            {
                Assert.IsFalse(array[i].CompareTo(array[i + 1]) > 0, nameof(array) + " is not sorted.");
            }
        }

        [TestMethod]
        public void QuickSortFullTest()
        {
            FileSystemManager manager = FileSystemManager.Get();
            CancellationToken cancellationToken = new();
            IList<IFileSystemEntry> entries = new List<IFileSystemEntry>();
            string path = @"C:\Users\julie\Documents";

            manager.GetFileSystemEntries(path, cancellationToken, ref entries, (IList<IFileSystemEntry> list, IFileSystemEntry entry) => { list.Add(entry); });

            IFileSystemEntry[] array = new IFileSystemEntry[entries.Count];
            entries.CopyTo(array, 0);

            QuickSort.Sort(array, 0, array.Length - 1);

            for (int i = 0; i < array.Length - 1; i++)
            {
                Assert.IsFalse(array[i].CompareTo(array[i + 1]) > 0, nameof(array) + " is not sorted.");
            }
        }

        [TestMethod]
        public void SortPerfTest()
        {
            List<int> randomList = new(100);
            Random rand = new();
            for (int i = 0; i < 100; i++)
            {
                randomList.Add(rand.Next());
            }

            for (int i = 0; i < 1000000; i++)
            {
                int[] array = new int[randomList.Count];
                randomList.CopyTo(array, 0);
                QuickSort.Sort(array, 0, array.Length - 1);
            }
        }
    }
}
