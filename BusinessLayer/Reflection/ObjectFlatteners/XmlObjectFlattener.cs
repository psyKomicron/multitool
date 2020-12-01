using System;
using System.Collections;
using System.Reflection;
using System.Xml;

namespace BusinessLayer.Reflection.ObjectFlatteners
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
            return RecursiveFlatten(o, new XmlDocument());
        }

        private XmlNode RecursiveFlatten<T>(T o, XmlDocument xml , string rootName = "properties", int depth = 0)
        {
            // check to prevent stack overflow
            if (depth > 100)
            {
                XmlNode xml1 = xml.CreateElement(rootName);
                xml1.InnerText = o.ToString();
                return xml1;
            }

            XmlNode root = xml.CreateElement(rootName);

            PropertyInfo[] propertyInfos = o.GetType().GetProperties();

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].MemberType == MemberTypes.Property)
                {
                    object value = propertyInfos[i].GetValue(o);
                    string name = propertyInfos[i].Name;

                    if (value != null)
                    {
                        if (IsPrimitive(value.GetType()))
                        {
                            XmlNode prop = xml.CreateElement(name);
                            prop.InnerText = value.ToString();
                            root.AppendChild(prop);
                        }
                        else if (IsEnumerable(propertyInfos[i].PropertyType))
                        {
                            IEnumerable array = propertyInfos[i].GetMethod.Invoke(o, null) as IEnumerable;
                            Type[] genericArgs = propertyInfos[i].PropertyType.GetGenericArguments();
                            Type itemType = null;
                            if (genericArgs.Length == 1)
                            {
                                itemType = genericArgs[0];
                            }

                            XmlNode arrayRoot = xml.CreateElement(name);
                            int index = 0;
                            foreach (var obj in array)
                            {
                                XmlNode xmlNode = xml.CreateElement(index.ToString());
                                if (itemType != null)
                                {
                                    var item = Convert.ChangeType(obj, itemType);
                                    
                                    if (IsPrimitive(item.GetType()))
                                    {
                                        xmlNode.InnerText = item.ToString();
                                    }
                                    else
                                    {
                                        XmlNode node = RecursiveFlatten(item, xml, item.GetType().Name, depth);
                                        xmlNode.AppendChild(node);
                                    }
                                    arrayRoot.AppendChild(xmlNode);
                                }
                                else
                                {
                                    xmlNode.InnerText = obj.ToString();
                                }
                                // update index for node names
                                index++;
                            }

                            root.AppendChild(arrayRoot);
                        }
                        else
                        {
                            XmlNode node = RecursiveFlatten(value, xml , name, depth + 1); // recursively flatten the properties of the object
                            root.AppendChild(node);
                        }
                    }
                    else
                    {
                        XmlNode node = RecursiveFlatten(value, xml, name, depth + 1); // recursively flatten the properties of the object
                        root.AppendChild(node);
                    }
                }
            }

            return root;
        }

    }
}
