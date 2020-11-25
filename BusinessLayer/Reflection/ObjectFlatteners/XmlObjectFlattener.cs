using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BusinessLayer.Reflection.ObjectFlatteners
{
    public class XmlObjectFlattener : IObjectFlattener<XmlNode>
    {
        public XmlNode Flatten<T>(T o) where T : class
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.CreateElement("properties");

            PropertyInfo[] propertyInfos = GetPropertyInfos<T>();

            for (int i = 0; i < propertyInfos.Length; i++)
            {

            }
        }

        private PropertyInfo[] GetPropertyInfos<T>()
        {
            return typeof(T).GetProperties();
        }
    }
}
