using Multitool.Reflection.ObjectFlatteners;
using Multitool.Reflection.PropertyLoaders;
using System;
using System.IO;
using System.Xml;

namespace Multitool.PreferencesManagers.Xml
{
    public class XmlPreferenceManager : IPreferenceManager
    {
        private XmlDocument xmlDocument;
        private XmlNode root;
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
                    root = xmlDocument.SelectSingleNode(".//PreferenceManager");
                }
                catch (XmlException e)
                {
                    // On error reset the file.
                    Console.Error.WriteLine(e.ToString());
                    File.WriteAllBytes(Path, new byte[0]);

                    root = xmlDocument.CreateElement("PreferenceManager");
                    xmlDocument.AppendChild(root);
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
                root = xmlDocument.CreateElement("PreferenceManager");
                xmlDocument.AppendChild(root);
            }
        }

        public void AddWindowData<DataType>(DataType data, string name) where DataType : class
        {
            XmlNode dataAsXml = new CommonXmlObjectFlattener().Flatten(data, typeof(DataType));
            XmlNode storedData = xmlDocument.SelectSingleNode(".//" + name);

            if (storedData != null)
            {
                root.RemoveChild(storedData);
            }
            
            XmlNode dataRoot = xmlDocument.CreateElement(name);
            foreach (XmlNode node in dataAsXml)
            {
                XmlNode importedNode = xmlDocument.ImportNode(node, true);
                dataRoot.AppendChild(importedNode);
            }
            root.AppendChild(dataRoot);
        }

        public DataType GetWindowData<DataType>(string name) where DataType : class, new()
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
