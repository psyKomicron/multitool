using BusinessLayer.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BusinessLayer.PreferencesManagers.Xml
{
    public class XmlPreferenceManager : IPreferenceManager
    {
        private const string _RootName = "PreferenceManager";
        private XmlDocument xmlDocument;
        private readonly IPropertyLoader propertyLoader = new PropertyLoader();

        public string Path { get; set; }

        /// <summary>
        /// Write the data stored in a XML file format on the hard drive.
        /// </summary>
        public void SerializePreferenceManager()
        {
            xmlDocument.Save(Path);
        }

        /// <summary>
        /// Load the values written on the disk as an XML file.
        /// </summary>
        public void DeserializePreferenceManager()
        {
            if (File.Exists(Path))
            {
                try
                {
                    xmlDocument = new XmlDocument();
                    xmlDocument.Load(Path);
                }
                catch (XmlException e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            }
            else
            {
                #region console
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Preference file does not exist at " + Path);
                Console.ResetColor();
                #endregion
                
                xmlDocument = new XmlDocument();
                XmlNode root = xmlDocument.CreateElement(_RootName);
                xmlDocument.AppendChild(root);
            }
        }

        public void AddWindowManager(WindowPreferenceManager manager)
        {
            XmlNode node = xmlDocument.SelectSingleNode(".//" + manager.ItemName);
            if (node != null)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    string nodeName = subNode.Name;
                    string nodeValue = subNode.FirstChild.InnerText;

                    if (!string.IsNullOrEmpty(nodeValue) && manager.Properties.ContainsKey(nodeName))
                    {
                        string storedValue = manager.Properties[nodeName];
                        if (!storedValue.Equals(nodeValue)) // edit the xml node to the value in manager
                        {
                            subNode.FirstChild.Value = storedValue;
                        }
                    }
                }
            }
            else // node is not existing, thus need to be created
            {
                AddToXmlDocument(xmlDocument.SelectSingleNode(".//" + _RootName), manager);
            }
        }

        public DataType GetWindowManager<DataType>(string name) where DataType : class, new()
        {
            if (xmlDocument != null)
            {
                XmlNode xmlManager = xmlDocument.SelectSingleNode(".//" + name);
                if (xmlManager != null)
                {
                    Dictionary<string, string> properties = ParseXml(xmlManager);
                    return propertyLoader.Load<DataType>(properties);
                }
                else
                {
                    return default;
                }
            }
            return default;
        }

        private void AddToXmlDocument(XmlNode root, WindowPreferenceManager manager)
        {
            XmlNode item = xmlDocument.CreateElement(manager.ItemName);
            foreach (KeyValuePair<string, string> pair in manager.Properties)
            {
                XmlNode property = xmlDocument.CreateElement(pair.Key);
                property.InnerText = pair.Value;
                item.AppendChild(property);
            }
            root.AppendChild(item);
        }

        private Dictionary<string, string> ParseXml(XmlNode xmlManager)
        {
            if (xmlManager != null)
            {
                Dictionary<string, string> properties = new Dictionary<string, string>();

                foreach (XmlNode node in xmlManager.ChildNodes)
                {
                    string nodeName = node.Name;
                    string nodeValue = node.FirstChild.Value;
                    properties.Add(nodeName, nodeValue);
                }

                return properties;
            }
            else
            {
                throw new ArgumentNullException("Argument xmlManager was null");
            }
        }

    }
}
