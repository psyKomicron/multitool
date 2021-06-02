using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.FileSystem;

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
            var o = new DirectorySizeCalculator();
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
            DirectorySizeCalculator o = new();
            Task<long> t = o.AsyncCalculateDirectorySize("D:\\", CancellationToken.None);
            t.Wait();
            Assert.AreEqual(o.CalculateDirectorySize("D:\\", CancellationToken.None), t.Result);
        }

        [TestMethod()]
        public void CalculateDirectorySizeTest()
        {
            long size = 150_466_322;
            DirectorySizeCalculator o = new();
            Assert.AreEqual(size, o.CalculateDirectorySize(@"D:\Monsieur_J\Documents\Code\Julie\Julie", CancellationToken.None));
        }
    }
}