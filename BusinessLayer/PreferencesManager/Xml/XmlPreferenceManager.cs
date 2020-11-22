using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace BusinessLayer.PreferencesManagers.Xml
{
    public class XmlPreferenceManager : IPreferenceManager<XmlWindowPreferenceManager>
    {
        private const string _RootName = "PreferenceManager";
        private XmlDocument xmlDocument;

        public string Path { get; set; }

        public void SerializePreferenceManager()
        {
            xmlDocument.Save(Path);
        }

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

        public XmlWindowPreferenceManager AddPreferenceManager(XmlWindowPreferenceManager manager)
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
                manager.AddToXmlDocument(xmlDocument, xmlDocument.SelectSingleNode(".//" + _RootName));
            }

            return manager;
        }

        public XmlWindowPreferenceManager GetWindowManager(string name)
        {
            if (xmlDocument != null)
            {
                XmlNode xmlManager = xmlDocument.SelectSingleNode(".//" + name);
                return xmlManager != null ? ParseXml(xmlManager) : null;
            }
            return null;
        }

        private XmlWindowPreferenceManager ParseXml(XmlNode xmlManager)
        {
            if (xmlManager != null)
            {
                string itemName = xmlManager.Name;
                Dictionary<string, string> properties = new Dictionary<string, string>();

                foreach (XmlNode node in xmlManager.ChildNodes)
                {
                    string nodeName = node.Name;
                    string nodeValue = node.FirstChild.Value;
                    properties.Add(nodeName, nodeValue);
                }

                return new XmlWindowPreferenceManager()
                {
                    ItemName = itemName,
                    Properties = properties
                };
            }
            else
            {
                throw new ArgumentNullException("XmlNode xmlManager provided was null");
            }
        }

    }
}
