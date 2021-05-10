using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiTool_Test;
using System.Xml;

namespace Multitool.Reflection.ObjectFlatteners.Tests
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