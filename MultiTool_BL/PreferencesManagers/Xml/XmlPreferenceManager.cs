using MultiToolBusinessLayer.Reflection.ObjectFlatteners;
using MultiToolBusinessLayer.Reflection.PropertyLoaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MultiToolBusinessLayer.PreferencesManagers.Xml
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
            XmlNode storedData = xmlDocument.SelectSingleNode(".//" + name);

            if (storedData != null)
            {
                for (int i = 0; i < dataAsXml.ChildNodes.Count; i++)
                {
                    XmlNode dataNode = dataAsXml.ChildNodes[i];
                    string nodeName = dataNode.Name;
                    string nodeValue = dataNode?.FirstChild?.InnerText;

                    XmlNode storedNode = storedData.SelectSingleNode(".//" + nodeName);

                    // Data object from the window is already stored
                    if (storedNode != null && !string.IsNullOrEmpty(nodeValue))
                    {
                        if (storedNode.ChildNodes != null && storedNode.ChildNodes[0]?.ChildNodes?.Count == 0)
                        {
                            string storedValueText = storedNode.FirstChild.InnerText;
                            if (storedValueText != null && !storedValueText.Equals(nodeValue)) // edit the xml node to the value in manager
                            {
                                storedNode.FirstChild.Value = nodeValue;
                            }
                        }
                        else
                        {
                            XmlNodeList incomingNodes = dataNode.ChildNodes;

                            for (int k = 0; k < incomingNodes.Count; k++)
                            {
                                XmlNode incomingNode = incomingNodes[k];
                                string incomingNodeValue = incomingNode.FirstChild.Value;

                                bool append = true;

                                foreach (XmlNode storedSubNode in storedNode.ChildNodes)
                                {
                                    string storedNodeValue = storedSubNode.FirstChild.Value;

                                    if (storedNodeValue.Equals(incomingNodeValue))
                                    {
                                        append = false;
                                        break;
                                    }
                                }
                                if (append)
                                {
                                    XmlNode importedNode = xmlDocument.ImportNode(incomingNode, true);
                                    storedNode.AppendChild(importedNode);
                                }
                            }
                        }
                    }
                    // update current node if current node value is empty and the incoming node has values
                    else if (storedNode != null && string.IsNullOrEmpty(storedNode.FirstChild?.Value) && dataNode != null)
                    {
                        XmlNodeList incomingNodes = dataNode.ChildNodes;
                        if (incomingNodes != null)
                        {
                            foreach (XmlNode incomingNode in incomingNodes)
                            {
                                XmlNode importedNode = xmlDocument.ImportNode(incomingNode, true);
                                storedNode.AppendChild(importedNode);
                            }
                        }
                    }
                    /*else if (storedNode == null && dataNode != null)
                    {

                    }*/
                }
            }
            else // node for data doesn't exist
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
