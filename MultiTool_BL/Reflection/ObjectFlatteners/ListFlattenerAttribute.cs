using System;
using System.Collections;
using System.Xml;

namespace Multitool.Reflection.ObjectFlatteners
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class ListFlattenerAttribute : Attribute, IObjectFlattener<XmlNode>
    {
        private XmlDocument xmlDocument = new XmlDocument();
        private string propertyName = string.Empty;
        private IObjectFlattener<XmlNode> flattener;

        public string PropertyName => propertyName;

        public ListFlattenerAttribute(string propertyName, Type type)
        {
            this.propertyName = propertyName;

            Type[] interfaces = type.FindInterfaces((Type m, object filter) =>
            {
                return m.Equals(typeof(IObjectFlattener<XmlNode>));
            }, null);

            if (interfaces.Length > 0)
            {
                flattener = (IObjectFlattener<XmlNode>)type.GetConstructor(new Type[0]).Invoke(null);
            }
            else
            {
                throw new ArgumentException(type.FullName + " does not implement " + nameof(IObjectFlattener<XmlNode>));
            }
        }

        public XmlNode Flatten(object o, Type objectType)
        {
            if (IsList(objectType))
            {
                XmlNode root = xmlDocument.CreateElement(propertyName);
                IEnumerable list = (IEnumerable)o;

                foreach (var item in list)
                {
                    XmlNode xmlItem = flattener.Flatten(item, item.GetType());
                    root.AppendChild(xmlDocument.ImportNode(xmlItem, true));
                }

                return root;
            }
            else
            {
                throw new ArgumentException(o.GetType().FullName + " does not implement IEnumerable");
            }
        }

        private bool IsList(Type type)
        {
            return ReflectionHelper.Implements<IEnumerable>(type);
        }
    }
}
