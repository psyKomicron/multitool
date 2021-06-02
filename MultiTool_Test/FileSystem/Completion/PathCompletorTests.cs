using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.FileSystem.Completion;

using System.Collections.Generic;
using System.IO;

namespace MultitoolTest.FileSystem.Completion.Tests
{
    [TestClass()]
    public class PathCompletorTests
    {
        [TestMethod()]
        public void PathCompletorTest()
        {
            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            PathCompletor[] pathCompletors = new PathCompletor[driveInfos.Length];
            for (int i = 0; i < driveInfos.Length; i++)
            {
                pathCompletors[i] = new PathCompletor();
            }
        }

        [TestMethod()]
        public void CompleteTest()
        {
            PathCompletor pathCompletor = new();
            List<string> completedPaths = new();
            pathCompletor.Complete(@"E:\julie\Videos\P\ev", completedPaths);
            Assert.AreEqual(2, completedPaths.Count);
        }
    }
}