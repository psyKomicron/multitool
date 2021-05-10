using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Multitool.Reflection.ObjectFlatteners
{
    [Obsolete]
    public class BasicObjectFlattener : IObjectFlattener<Dictionary<string, string>>
    {
        public Dictionary<string, string> Flatten(object o, Type objectType)
        {
            Dictionary<string, string> flatProperties = new Dictionary<string, string>();
            PropertyInfo[] properties = ReflectionHelper.GetPropertyInfos(objectType);

            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].MemberType == MemberTypes.Property)
                {
                    string key = properties[i].Name;
                    if (properties[i].GetValue(o) != null)
                    {
                        object value = properties[i].GetValue(o);
                        if (value.GetType().IsPrimitive || value.GetType() == typeof(string))
                        {
                            flatProperties.Add(key, properties[i].GetValue(o).ToString());
                        }
                        else 
                        {
                            Dictionary<string, string> props = Flatten(o, value.GetType());
                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (var prop in props)
                            {
                                stringBuilder.Append("[" + prop.Key).Append(":").Append(prop.Value).Append("]");
                            }
                            flatProperties.Add(key, stringBuilder.ToString());
                        }
                    }
                }
            }

            return flatProperties;
        }
    }
}
