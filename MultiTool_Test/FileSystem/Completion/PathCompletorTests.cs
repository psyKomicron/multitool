using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;

namespace Multitool.FileSystem.Completion.Tests
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
            pathCompletor.Complete(@"E:\julie\Videos\Desktop\P\ev", completedPaths);
            Assert.IsTrue(completedPaths.Count > 0);
        }
    }
}