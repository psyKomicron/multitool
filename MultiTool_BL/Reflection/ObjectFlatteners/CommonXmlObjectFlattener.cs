using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;

namespace Multitool.Reflection.ObjectFlatteners
{
    public class CommonXmlObjectFlattener : IObjectFlattener<XmlNode>
    {
        /// <summary>
        /// Transforms a object/class to xml. The properties are under the "properties" node.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public XmlNode Flatten(object o, Type objectType)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                return RecursiveFlatten(o, xmlDocument, objectType.Name);
            }
            catch (ArgumentNullException ane)
            {
                Console.Error.WriteLine(ane);
                return xmlDocument.CreateElement("properties");
            }
        }

        private XmlNode RecursiveFlatten<T>(T o, XmlDocument xml , string rootName, int depth = 0)
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
                throw new ArgumentNullException(nameof(o), "Value provided to flatten was null: \n{ rootName: " + rootName + ", depth: " + depth + " }");
            }
            #endregion

            XmlNode root = xml.CreateElement(rootName);
            PropertyInfo[] propertyInfos = ReflectionHelper.GetPropertyInfos(o.GetType());

            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].MemberType == MemberTypes.Property)
                {
                    object value = null;
                    string name = propertyInfos[i].Name;

                    try
                    {
                        value = propertyInfos[i].GetValue(o);
                    }
                    catch (TargetInvocationException tie)
                    {
                        Console.WriteLine(tie.ToString());
                    }

                    // get flattener attribute if existing
                    Attribute flattenerAttribute = null;
                    IEnumerable<Attribute> attributes = propertyInfos[i].GetCustomAttributes();
                    foreach (Attribute attribute in attributes)
                    {
                        if (ReflectionHelper.Implements<IObjectFlattener<XmlNode>>(attribute.GetType()))
                        {
                            flattenerAttribute = attribute;
                            break;
                        }
                    }
                    if (value != null)
                    {
                        if (flattenerAttribute == null)
                        {
                            if (ReflectionHelper.IsPrimitiveType(value.GetType()))
                            {
                                AppendAndCreateNode(xml, root, value, name);
                            }
                            else if (ReflectionHelper.Implements(propertyInfos[i].PropertyType, typeof(IEnumerable)))
                            {
                                IEnumerable array = propertyInfos[i].GetMethod.Invoke(o, null) as IEnumerable;
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
                                            if (ReflectionHelper.IsPrimitiveType(item.GetType()) || CanTreatObject(itemType))
                                            {
                                                AppendAndCreateNode(xml, arrayRoot, item, index);
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
                                        AppendAndCreateNode(xml, arrayRoot, obj, index);
                                    }
                                }

                                root.AppendChild(arrayRoot);
                            }
                            else if (CanTreatObject(value.GetType()))
                            {
                                AppendAndCreateNode(xml, root, value, name);
                            }
                            else
                            {
                                XmlNode node = RecursiveFlatten(value, xml, name, depth + 1); // recursively flatten the properties of the object
                                root.AppendChild(node);
                            }
                        }
                        else
                        {
                            XmlNode node = ((IObjectFlattener<XmlNode>)flattenerAttribute).Flatten(value, propertyInfos[i].PropertyType);
                            if (node != null)
                            {
                                root.AppendChild(xml.ImportNode(node, true));
                            }
                        }
                    }
                    else
                    {
                        AppendAndCreateNode(xml, root, string.Empty, name);
                    }
                }
            }

            return root;
        }

        private bool CanTreatObject(Type oType)
        {
            if (oType != null)
            {
                return oType.Equals(typeof(DateTime)) || oType.Name.Equals("SolidColorBrush");
            }
            return false;
        }

        private void AppendAndCreateNode(XmlDocument xml, XmlNode root, object value, string name)
        {
            XmlNode node = xml.CreateElement(name);
            node.InnerText = value.ToString();
            root.AppendChild(node);
        }
    }
}
