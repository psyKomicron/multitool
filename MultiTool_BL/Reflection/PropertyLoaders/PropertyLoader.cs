﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Multitool.Reflection.PropertyLoaders
{
    public class PropertyLoader
    {
        public bool HaltOnException { get; set; }

        [Obsolete]
        public DtoType LoadFromStringDictionary<DtoType>(Dictionary<string, string> dictionary) where DtoType : class, new()
        {
            DtoType dto = new DtoType();
            PropertyInfo[] properties = ReflectionHelper.GetPropertyInfos<DtoType>();

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

        private object RecursiveLoadFromXml(XmlNode xml, Type dtoType)
        {
            ConstructorInfo constructor = dtoType.GetConstructor(new Type[0]);
            if (constructor != null)
            {
                var dto = constructor.Invoke(null);
                // match object property names with xml nodes names
                // creating dictionnary with property names and actual property methods
                Dictionary<string, PropertyInfo> properties = ToDictionnary(dtoType.GetProperties());

                XmlNodeList nodes = xml.ChildNodes;
                foreach (XmlNode node in nodes)
                {
                    string nodeName = node.Name;
                    if (properties.ContainsKey(nodeName))
                    {
                        PropertyInfo property = properties[nodeName];
                        if (node.HasChildNodes && node.ChildNodes.Count == 1 && !node.FirstChild.HasChildNodes)
                        {
                            string nodeValue = node.FirstChild.InnerText;
                            if (!string.IsNullOrEmpty(nodeValue))
                            {
                                SafeLoadIntoProperty(property, dto, nodeValue);
                            }
                        }
                        else if (node.HasChildNodes)
                        {
                            if (ReflectionHelper.Implements<IList>(property.PropertyType))
                            {
                                SafeInstanciateProperty(property, dto);
                                Type generic = property.PropertyType.GetGenericArguments()[0];

                                XmlNodeList childNodes = node.ChildNodes;

                                IList list = property.GetValue(dto) as IList;

                                foreach (XmlNode listItem in childNodes)
                                {
                                    if (listItem.Name != generic.Name)
                                    {
                                        throw new ArrayTypeMismatchException();
                                    }

                                    if (listItem.HasChildNodes)
                                    {
                                        var item = Convert.ChangeType(RecursiveLoadFromXml(listItem, generic), generic);
                                        list.Add(item);
                                    }
                                }
                            }
                            else
                            {
                                // recursive, should instanciate object and properties
                                var data = RecursiveLoadFromXml(node, property.PropertyType);
                                SafeLoadIntoProperty(property, dto, data);
                            }
                        }
                    }
                }

                return dto;
            }
            else
            {
                if (dtoType.Equals(typeof(string)))
                {
                    return xml.FirstChild?.Value;
                }
                else if (dtoType.IsPrimitive)
                {
                    return Convert.ChangeType(xml.FirstChild?.Value, dtoType);
                }
                return null;
            }
        }

        private void SafeInstanciateProperty(PropertyInfo property, object target)
        {
            Type type = property.PropertyType;

            ConstructorInfo constructor = type.GetConstructor(new Type[0]);

            if (constructor != null)
            {
                var o = constructor.Invoke(null);
                try
                {
                    var value = Convert.ChangeType(o, type);
                    property.SetValue(target, value);
                }
                catch (InvalidCastException ice)
                {
                    if (HaltOnException) throw;
                    HandleICE(ice, property);
                }
                catch (TargetInvocationException tie)
                {
                    if (HaltOnException) throw;
                    HandleTIE(tie, property);
                }
            }
        }

        private void SafeLoadIntoProperty(PropertyInfo property, object target, object propValue)
        {
            Type type = property.PropertyType;

            // safely instanciate the property
            if (propValue != null)
            {
                try
                {
                    var value = Convert.ChangeType(propValue, type);
                    property.SetValue(target, value);
                }
                catch (InvalidCastException ice)
                {
                    if (HaltOnException) throw;
                    HandleICE(ice, property);
                }
                catch (TargetInvocationException tie)
                {
                    if (HaltOnException) throw;
                    HandleTIE(tie, property);
                }
            }
            else
            {
                // will always work because of the new() constraint
                SafeInstanciateProperty(property, target);
            }
        }

        private Dictionary<string, PropertyInfo> ToDictionnary(PropertyInfo[] propertyInfos)
        {
            Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                properties.Add(propertyInfos[i].Name, propertyInfos[i]);
            }
            return properties;
        }

        #region exceptions handlers
        private void HandleTIE(TargetInvocationException tie, PropertyInfo property)
        {
            Console.BackgroundColor = ConsoleColor.Red;

            Console.WriteLine(GetType().Name + " was not able to set the value of property : " + property.Name);
            Console.Error.WriteLine(tie.ToString());

            Console.ResetColor();
        }

        private void HandleICE(InvalidCastException ice, PropertyInfo property)
        {
            Console.BackgroundColor = ConsoleColor.Red;

            Console.WriteLine(GetType().Name + " was not able to convert the value of property : " + property.Name);
            Console.Error.WriteLine(ice.ToString());

            Console.ResetColor();
        }
        #endregion
    }
}
