using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.Optimisation;

using System;

namespace MultitoolTest.Optimisation.Tests
{
    [TestClass()]
    public class CircularBufferTests
    {
        [TestMethod()]
        public void CircularBufferTest()
        {
            var buff = new CircularBag<int>(10);
        }

        [TestMethod()]
        public void AddTest()
        {
            var buff = new CircularBag<int>(10);

            for (int i = 0; i < 20; i++)
            {
                buff.Add(i);
            }

            for (int i = 0; i < buff.Length; i++)
            {
                Console.WriteLine(buff[i]);
            }
        }
    }
}