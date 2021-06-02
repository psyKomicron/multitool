using Microsoft.VisualStudio.TestTools.UnitTesting;
using Multitool.Reflection.ObjectFlatteners;
using Multitool.Reflection.PropertyLoaders;

using MultiToolTest;

using System.Xml;

namespace MultitoolTest.Reflection.PropertyLoaders.Tests
{
    [TestClass]
    public class PropertyLoaderTests
    {
        [TestMethod]
        public void LoadFromStringDictionaryTest()
        {
            throw new AssertInconclusiveException();
        }

        [TestMethod]
        public void LoadFromXmlTest()
        {
            var o = new TestClass();
            var flattener = new CommonXmlObjectFlattener();

            XmlNode xml = flattener.Flatten(o, typeof(TestClass));

            PropertyLoader loader = new PropertyLoader();
            var test = loader.LoadFromXml<TestClass>(xml);

            Assert.IsTrue(o.Equals(test));
        }
    }
}