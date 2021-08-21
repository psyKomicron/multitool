using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.JulieV2;
using Multitool.Reflection.ObjectFlatteners;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace MultitoolTest.Reflection.ObjectFlatteners.Tests
{
    [TestClass]
    public class XmlListFlattenerAttributeTests
    {
        [TestMethod]
        public void XmlListFlattenerAttributeTest()
        {
            ListFlattenerAttribute attribute = new("Attributes", typeof(PrimitiveXmlFlattener));
            Assert.IsNotNull(attribute);
            Assert.IsTrue(attribute.PropertyName == "Attributes");
        }

        [TestMethod]
        public void FlattenTest()
        {
            PrimitiveFlattenTest();
            ObjectFlattenTest();
        }

        [TestMethod]
        public void PrimitiveFlattenTest()
        {
            var o = new ForAttributeTest();
            PropertyInfo info = o.GetType().GetProperty("Items");
            Attribute attribute = info.GetCustomAttribute(typeof(ListFlattenerAttribute));

            Assert.IsTrue(attribute.GetType() == typeof(ListFlattenerAttribute));
            Assert.AreEqual("Items", ((ListFlattenerAttribute)attribute).PropertyName);

            var value = info.GetValue(o);
            Type propType = info.PropertyType;

            XmlNode node = ((ListFlattenerAttribute)attribute).Flatten(value, propType);

            if (node.ChildNodes != null && node.ChildNodes.Count == 4)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    Assert.AreEqual((i + 1).ToString(), node.ChildNodes[i].InnerText);
                }
            }
            else
            {
                throw new AssertFailedException("Object not properly flattened. Expected 4 child elements");
            }
        }

        [TestMethod]
        public void ObjectFlattenTest()
        {
            var o = new ForAttributeTest();
            PropertyInfo info = o.GetType().GetProperty("Users");
            Attribute attribute = info.GetCustomAttribute(typeof(ListFlattenerAttribute));

            Assert.IsTrue(attribute.GetType() == typeof(ListFlattenerAttribute));
            Assert.AreEqual("Users", ((ListFlattenerAttribute)attribute).PropertyName);

            XmlNode node = ((ListFlattenerAttribute)attribute).Flatten(info.GetValue(o), info.PropertyType);
            Assert.IsNotNull(node);

            if (node.ChildNodes != null && node.ChildNodes.Count == 4)
            {
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    Assert.AreEqual(o.Users[i].Discriminator.ToString(), node.ChildNodes[i].SelectSingleNode(".//Discriminator").InnerText);
                    Assert.AreEqual(o.Users[i].Tag, node.ChildNodes[i].SelectSingleNode(".//Tag").InnerText);
                    Assert.AreEqual(o.Users[i].Name, node.ChildNodes[i].SelectSingleNode(".//Name").InnerText);
                }
            }
            else
            {
                throw new AssertFailedException("Object not properly flattened. Expected 4 child elements");
            }
        }
    }

    class ForAttributeTest
    {
        [ListFlattener(nameof(Items), typeof(PrimitiveXmlFlattener))]
        public List<string> Items { get; set; }

        [ListFlattener(nameof(Users), typeof(CommonXmlObjectFlattener))]
        public List<User> Users { get; set; }

        public ForAttributeTest()
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
    }
}