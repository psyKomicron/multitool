using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.Reflection.ObjectFlatteners;

using MultiToolTest;
using System.Xml;

namespace MultitoolTest.Reflection.ObjectFlatteners.Tests
{
    [TestClass()]
    public class CommonXmlObjectFlattenerTests
    {
        [TestMethod()]
        public void FlattenTest()
        {
            var o = new TestClass();
            var flattener = new CommonXmlObjectFlattener();
            XmlNode flat = flattener.Flatten(o, o.GetType());
            Assert.IsTrue(o.EqualsXml(flat));
        }
    }
}