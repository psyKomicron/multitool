using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BusinessLayer.Reflection.ObjectFlatteners
{
    public class BasicObjectFlattener : ObjectFlattener<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Flatten<T>(T o)
        {
            Dictionary<string, string> flatProperties = new Dictionary<string, string>();
            PropertyInfo[] properties = GetPropertyInfos<T>();

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
                            Dictionary<string, string> props = Flatten(o);
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
