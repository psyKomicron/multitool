using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace MultiToolBusinessLayer.Reflection.ObjectFlatteners
{
    public class XmlObjectFlattener : ObjectFlattener<XmlNode>
    {
        /// <summary>
        /// Transforms a object/class to xml. The properties are under the "properties" node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public override XmlNode Flatten<T>(T o)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                return RecursiveFlatten(o, xmlDocument);
            }
            catch (ArgumentNullException ane)
            {
                Console.Error.WriteLine(ane);
                return xmlDocument.CreateElement("properties");
            }
            
        }

        private XmlNode RecursiveFlatten<T>(T o, XmlDocument xml , string rootName = "properties", int depth = 0)
        {
            #region checks
            // check to prevent stack overflow
            if (depth >= 10)
            {
                XmlNode xml1 = xml.CreateElement(rootName);
                xml1.InnerText = o.ToString();
                return xml1;
            }

            if (o == null)
            {
                // "Value provided to flatten was null: \n[rootName: " + rootName + ",\ndepth: " + depth
                throw new ArgumentNullException("o", "Value provided to flatten was null: \n{ rootName: " + rootName + ", depth: " + depth + " }");
            }
            #endregion

            XmlNode root = xml.CreateElement(rootName);

            PropertyInfo[] propertyInfos = o.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].MemberType == MemberTypes.Property)
                {
                    object value = null;
                    try
                    {
                        value = propertyInfos[i].GetValue(o);
                    }
                    catch (TargetInvocationException tie)
                    {
                        Console.WriteLine(tie.ToString());
                    }

                    string name = propertyInfos[i].Name;

                    if (value != null)
                    {
                        if (IsPrimitive(value.GetType()))
                        {
                            XmlNode prop = xml.CreateElement(name);
                            prop.InnerText = value.ToString();
                            root.AppendChild(prop);
                        }
                        else if (IsList(propertyInfos[i].PropertyType))
                        {
                            IList array = propertyInfos[i].GetMethod.Invoke(o, null) as IList;
                            Type[] genericArgs = propertyInfos[i].PropertyType.GetGenericArguments();
                            Type itemType = null;

                            if (genericArgs.Length > 0)
                            {
                                itemType = genericArgs[0];
                            }

                            XmlNode arrayRoot = xml.CreateElement(name);
                            string index = itemType.Name;

                            foreach (var obj in array)
                            {
                                if (itemType != null)
                                {
                                    var item = Convert.ChangeType(obj, itemType);

                                    if (item != null)
                                    {
                                        if (IsPrimitive(item.GetType()) || CanTreatObject(itemType))
                                        {
                                            XmlNode xmlNode = xml.CreateElement(index);
                                            xmlNode.InnerText = item.ToString();
                                            arrayRoot.AppendChild(xmlNode);
                                        }
                                        else
                                        {
                                            XmlNode node = RecursiveFlatten(item, xml, item.GetType().Name, depth + 1);
                                            arrayRoot.AppendChild(node);
                                        }
                                    }
                                }
                                else
                                {
                                    XmlNode xmlNode = xml.CreateElement(index);
                                    xmlNode.InnerText = obj.ToString();
                                    arrayRoot.AppendChild(xmlNode);
                                }
                            }

                            root.AppendChild(arrayRoot);
                        }
                        else if (CanTreatObject(value.GetType()))
                        {
                            XmlNode node = xml.CreateElement(name);
                            node.InnerText = value.ToString();
                            root.AppendChild(node);
                        }
                        else
                        {
                            XmlNode node = RecursiveFlatten(value, xml , name, depth + 1); // recursively flatten the properties of the object
                            root.AppendChild(node);
                        }
                    }
                }
            }

            return root;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oType"></param>
        /// <returns></returns>
        private bool CanTreatObject(Type oType)
        {
            if (oType != null)
            {
                return oType.Equals(typeof(DateTime)) || oType.Name.Equals("SolidColorBrush");
            }
            return false;
        }

    }
}
