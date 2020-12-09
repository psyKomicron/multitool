using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace BusinessLayer.Reflection.PropertyLoaders
{
    internal class PropertyLoader
    {
        public DtoType LoadFromStringDictionary<DtoType>(Dictionary<string, string> dictionary) where DtoType : class, new()
        {
            DtoType dto = new DtoType();
            PropertyInfo[] properties = GetProperties<DtoType>();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];

                if (dictionary.ContainsKey(property.Name))
                {
                    string propValue = dictionary[property.Name];
                    SafeLoadIntoProperty(property, dto, propValue);
                }
            }

            return dto;
        }

        public DtoType LoadFromXml<DtoType>(XmlNode xml) where DtoType : class, new()
        {
            return RecursiveLoadFromXml(xml, typeof(DtoType)) as DtoType;
        }

        private object RecursiveLoadFromXml(XmlNode xml, Type dtoType = null)
        {
            var dto = dtoType.GetConstructor(new Type[0]).Invoke(null);

            // match object property names with xml nodes names
            // creating dictionnary with property names and actual property methods
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            PropertyInfo[] propertyInfos = dtoType.GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                properties.Add(propertyInfos[i].Name, propertyInfos[i]);
            }

            XmlNodeList nodes = xml.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                string nodeName = node.Name;
                if (properties.ContainsKey(nodeName))
                {
                    PropertyInfo property = properties[nodeName];
                    if (!node.HasChildNodes)
                    {
                        string nodeValue = node.Value;
                        if (!string.IsNullOrEmpty(nodeValue))
                        {
                            SafeLoadIntoProperty(property, dto, nodeValue);
                        }
                    }
                    else
                    {
                        Type type = property.PropertyType;
                        var data = RecursiveLoadFromXml(node, type); // recursive, should instanciate object and properties
                        SafeLoadIntoProperty(property, dto, data);
                    }
                }
            }

            return dto;
        }

        private void SafeLoadIntoProperty(PropertyInfo property, object dto, object propValue)
        {
            Type type = property.PropertyType;

            // safely instanciate the property
            if (propValue != null)
            {
                try
                {
                    var value = Convert.ChangeType(propValue, type);
                    property.SetValue(dto, value);
                }
                catch (InvalidCastException ice)
                {
                    Console.BackgroundColor = ConsoleColor.Red;

                    Console.WriteLine(GetType().Name + " was not able to convert the value of property : " + property.Name);
                    Console.Error.WriteLine(ice.ToString());

                    Console.ResetColor();
                }
                catch (TargetInvocationException tie)
                {
                    Console.BackgroundColor = ConsoleColor.Red;

                    Console.WriteLine(GetType().Name + " was not able to set the value of property : " + property.Name);
                    Console.Error.WriteLine(tie.ToString());

                    Console.ResetColor();
                }
            }
            else
            {
                // will always work because of the new() constraint
                var defaultValue = type.GetConstructor(new Type[0]).Invoke(null);
                property.SetValue(dto, defaultValue);
            }
        }

        private PropertyInfo[] GetProperties<T>() where T : class
        {
            return typeof(T).GetProperties();
        }
    }
}
