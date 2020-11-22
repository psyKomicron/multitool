using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace BusinessLayer.PreferencesManagers.Xml
{
    public class XmlWindowPreferenceManager : WindowPreferenceManager, IEquatable<XmlWindowPreferenceManager>
    {
        public void AddToXmlDocument(XmlDocument document, XmlNode root)
        {
            XmlNode item = document.CreateElement(ItemName);
            foreach (KeyValuePair<string, string> pair in Properties)
            {
                XmlNode property = document.CreateElement(pair.Key);
                property.InnerText = pair.Value;
                item.AppendChild(property);
            }
            root.AppendChild(item);
        }

        public bool Equals(XmlWindowPreferenceManager other)
        {
            return base.Equals(other);
        }
    }
}
