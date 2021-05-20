using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.Optimisation;

using MultiTool_Test;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.Optimisation.Tests
{
    [TestClass()]
    public class ObjectPoolTests
    {
        [TestMethod()]
        public void ObjectPoolTest()
        {
            var o = new ObjectPool<TestClass>();
        }

        [TestMethod()]
        public void ObjectPoolTest1()
        {
            var o = new ObjectPool<TestClass>(10000);
        }

        [TestMethod()]
        public void ObjectPoolTest2()
        {
            var o = new ObjectPool<TestClass>(Array.Empty<object>());
            Assert.ThrowsException<ArgumentException>(() => new ObjectPool<TestClass>(true));
        }

        [TestMethod()]
        public void ObjectPoolTest3()
        {
            var o = new ObjectPool<TestClass>(10, Array.Empty<object>());
            Assert.ThrowsException<ArgumentException>(() => new ObjectPool<TestClass>(10, true));
        }

        [TestMethod()]
        public void GetObjectTest()
        {
            int max = 10_000_000;
            ObjectPool<TestClass> objectPool = new(max);
            TestClass[] classes = new TestClass[max];

            for (int i = 0; i < max; i++)
            {
                classes[i] = objectPool.GetObject();
            }

            TestClass c = objectPool.GetObject();

            Random rand = new Random();
            for (int i = 0; i < max; i++)
            {
                int n = rand.Next() % max;
                classes[n].InUse = false;
            }

            for (int i = 0; i < max - 1; i++)
            {
                classes[i] = objectPool.GetObject();
            }
        }
    }
}