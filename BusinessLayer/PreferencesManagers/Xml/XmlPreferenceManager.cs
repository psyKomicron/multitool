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
                xmlDocument = new XmlDocument();
                try
                {
                    xmlDocument.Load(Path);
                }
                catch (XmlException e)
                {
                    // On error reset the file.
                    Console.Error.WriteLine(e.ToString());
                    File.WriteAllBytes(Path, new byte[0]);

                    xmlDocument.AppendChild(xmlDocument.CreateElement(_RootName));
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
                            XmlNode storedValue = dataAsXml.SelectSingleNode(".//" + nodeName);
                            string storedValueText = storedValue?.FirstChild.InnerText;
                            if (storedValueText != null && !storedValueText.Equals(nodeValue)) // edit the xml node to the value in manager
                            {
                                subNode.FirstChild.Value = storedValueText;
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
                    // update current node if current node value is empty and the incoming node has values
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
                    return propertyLoader.LoadFromXml<DataType>(xmlManager);
                }
                else
                {
                    return new DataType();
                }
            }
            return new DataType();
        }

    }
}
