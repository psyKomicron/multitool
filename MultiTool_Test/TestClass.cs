using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.JulieV2;
using Multitool.Optimisation;
using Multitool.Reflection.ObjectFlatteners;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace MultiToolTest
{
    [TestClass]
    public class TestClass : IEquatable<TestClass>, IPoolableObject
    {
        private bool _inUse;

        public TestClass()
        {
            /*Items = new() { "1", "2", "3", "4" };
            Users = new()
            {
                UserFactory.Create(6527, "psyKomicron", "psyKomicron"),
                UserFactory.Create(6528, "psyKomicron", "Julie"),
                UserFactory.Create(6529, "JollyJoker", "psyKomicron"),
                UserFactory.Create(6530, "JulieV2", "Nono the robot")
            };*/
        }

        [ListFlattener(nameof(Items), typeof(PrimitiveXmlFlattener))]
        public List<string> Items { get; set; }

        [ListFlattener(nameof(Users), typeof(CommonXmlObjectFlattener))]
        public List<User> Users { get; set; }

        public string P1 { get; set; }
        public int P2 { get; set; }
        public object P3 { get; set; }
        public bool InUse
        {
            get => _inUse;
            set
            {
                _inUse = value;
                if (!_inUse)
                {
                    Free?.Invoke(this);
                }
            }
        }

        public event FreeObjectEventHandler Free;

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
            PropertyInfo[] infos = GetType().GetProperties();
            foreach (var info in infos)
            {
                Console.WriteLine(info.ToString());
            }
        }
    }
}
