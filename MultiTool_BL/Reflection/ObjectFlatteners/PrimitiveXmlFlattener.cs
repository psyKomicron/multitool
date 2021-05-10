using System;
using System.Xml;

namespace Multitool.Reflection.ObjectFlatteners
{
    public class PrimitiveXmlFlattener : IObjectFlattener<XmlNode>
    {
        private XmlDocument document = new XmlDocument();

        public XmlNode Flatten(object o, Type objectType)
        {
            if (ReflectionHelper.IsPrimitiveType(objectType))
            {
                XmlNode node = document.CreateElement(o.GetType().Name);
                node.InnerText = o.ToString();
                return node;
            }
            else
            {
                throw new ArgumentException("Type is not valid", nameof(o));
            }
        }
    }
}
