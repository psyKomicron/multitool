using Microsoft.VisualStudio.TestTools.UnitTesting;
using Multitool.JulieV2;
using Multitool.Reflection.ObjectFlatteners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MultiTool_Test
{
    [TestClass]
    public class TestClass : IEquatable<TestClass>
    {
        [ListFlattener(nameof(Items), typeof(PrimitiveXmlFlattener))]
        public List<string> Items { get; set; }

        [ListFlattener(nameof(Users), typeof(CommonXmlObjectFlattener))]
        public List<User> Users { get; set; }

        public TestClass()
        {
            Items = new() { "1", "2", "3", "4" };
            Users = new()
            {
                UserFactory.Create(6527, "psyKomicron", "psyKomicron"),
                UserFactory.Create(6528, "psyKomicron", "Julie"),
                UserFactory.Create(6529, "JollyJoker", "psyKomicron"),
                UserFactory.Create(6530, "JulieV2", "Nono the robot")
            };
        }

        public bool EqualsXml(XmlNode node)
        {
            Assert.AreEqual(nameof(TestClass), node.Name);
            XmlNode items = node.SelectSingleNode(".//Items");
            XmlNode users = node.SelectSingleNode(".//Users");

            Assert.IsNotNull(items);
            Assert.IsNotNull(users);

            for (int i = 0; i < items.ChildNodes.Count; i++)
            {
                Assert.AreEqual((i + 1).ToString(), items.ChildNodes[i].InnerText);
            }

            Assert.AreEqual(Users.Count, users.ChildNodes.Count);
            for (int i = 0; i < Users.Count; i++)
            {
                XmlNode childNode = users.ChildNodes[i];
                string discriminator, tag, name;
                discriminator = childNode.SelectSingleNode(".//Discriminator").InnerText;
                tag = childNode.SelectSingleNode(".//Tag").InnerText;
                name = childNode.SelectSingleNode(".//Name").InnerText;

                Assert.IsNotNull(discriminator);
                Assert.IsNotNull(tag);
                Assert.IsNotNull(name);

                Assert.AreEqual(Users[i].Discriminator.ToString(), discriminator);
                Assert.AreEqual(Users[i].Tag, tag);
                Assert.AreEqual(Users[i].Name, name);
            }

            return true;
        }

        public bool Equals(TestClass other)
        {
            return Items.SequenceEqual(other.Items) && Users.SequenceEqual(other.Users);
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var array1 = new List<TestClass>()
            {
                new TestClass(),
                new TestClass(),
                new TestClass(),
                new TestClass(),
                new TestClass()
            };
            TestClass[] classes = new TestClass[array1.Count];
            array1.CopyTo(classes, 0);
            for (int i = 0; i < array1.Count; i++)
            {
                var item = array1[i];
                var copy = classes[i];
                Assert.AreSame(item, copy);
            }
        }
    }
}
