using BusinessLayer.Reflection.ObjectFlatteners;
using BusinessLayer.Reflection.PropertyLoaders;
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
        private readonly PropertyLoader propertyLoader = new PropertyLoader();

        public string Path { get; set; }

        public PreferenceManagerType Type => PreferenceManagerType.XML;

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
                    // check if the root is created
                    if (xmlDocument.SelectSingleNode(".//" + _RootName) == null)
                    {
                        xmlDocument.AppendChild(xmlDocument.CreateElement(_RootName));
                    }
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

        public void AddWindowManager<DataType>(DataType data, string name) where DataType : class
        {
            XmlNode dataAsXml = new XmlObjectFlattener().Flatten(data);
            XmlNode node = xmlDocument.SelectSingleNode(".//" + name);
            if (node != null)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    string nodeName = subNode.Name;
                    string nodeValue = subNode?.FirstChild?.InnerText;

                    if (!string.IsNullOrEmpty(nodeValue) && dataAsXml.SelectSingleNode(".//" + nodeName) != null)
                    {
                        bool isList = dataAsXml.SelectSingleNode(".//" + nodeName).ChildNodes?.Count > 1; // count >= 2

                        if (!isList)
                        {
                            XmlNode storedValue = xmlDocument.ImportNode(dataAsXml.SelectSingleNode(".//" + nodeName).FirstChild, true);
                            if (storedValue != null && !storedValue.Equals(nodeValue)) // edit the xml node to the value in manager
                            {
                                node.RemoveChild(subNode);
                                node.AppendChild(storedValue);
                            }
                        }
                        else
                        {
                            XmlNodeList incomingNodes = dataAsXml.SelectSingleNode(".//" + nodeName).ChildNodes;
                            foreach (XmlNode incomingNode in incomingNodes)
                            {
                                XmlNode importedNode = xmlDocument.ImportNode(incomingNode, true);
                                subNode.AppendChild(importedNode);
                            }
                        }
                    }   
                    // update current node if the incoming node has values
                    else if (string.IsNullOrEmpty(nodeValue) && dataAsXml.SelectSingleNode(".//" + nodeName) != null)
                    {
                        XmlNodeList incomingNodes = dataAsXml.SelectSingleNode(".//" + nodeName).ChildNodes;
                        if (incomingNodes != null)
                        {
                            foreach (XmlNode incomingNode in incomingNodes)
                            {
                                XmlNode importedNode = xmlDocument.ImportNode(incomingNode, true);
                                subNode.AppendChild(importedNode);
                            }
                        }
                    }
                }
            }
            else // node is not existing
            {
                XmlNode dataRoot = xmlDocument.CreateElement(name);
                foreach (XmlNode childNode in dataAsXml.ChildNodes)
                {
                    XmlNode importNode = xmlDocument.ImportNode(childNode, true);
                    dataRoot.AppendChild(importNode);
                }

                xmlDocument.SelectSingleNode(".//" + _RootName).AppendChild(dataRoot);
            }
        }

        public DataType GetWindowManager<DataType>(string name) where DataType : class, new()
        {
            if (xmlDocument != null)
            {
                XmlNode xmlManager = xmlDocument.SelectSingleNode(".//" + name);
                if (xmlManager != null)
                {
                    //Dictionary<string, string> properties = ParseXml(xmlManager);
                    return propertyLoader.LoadFromXml<DataType>(xmlManager);
                }
                else
                {
                    return new DataType();
                }
            }
            return new DataType();
        }

        #region private methods
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
                    string nodeValue = string.Empty;
                    if (node.ChildNodes.Count > 0)
                    {
                        nodeValue = node.FirstChild.Value;
                    }
                    properties.Add(nodeName, nodeValue);
                }

                return properties;
            }
            else
            {
                throw new ArgumentNullException("Argument xmlManager was null");
            }
        }
        #endregion

    }
}
