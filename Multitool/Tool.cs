using BusinessLayer.PreferencesManager;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MultiTool
{
    internal static class Tool
    {
        private static IPreferenceManager<JsonWindowPreferenceManager> preferenceManager;

        public static IPreferenceManager<JsonWindowPreferenceManager> GetPreferenceManager() => preferenceManager;

        public static void SetPreferenceManagerPath(string path)
        {
            if (preferenceManager != null)
            {
                preferenceManager.Path = path;
            }
            else
            {
                preferenceManager = new JsonPreferenceManager(path);
            }
        }

        public static void FireEvent(PropertyChangedEventHandler propertyChanged, object source, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(source, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// ToString but for properties of an object. Only flatten if the property is a primitive type and a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Flatten<T>(T data) where T : class
        {
            Dictionary<string, string> flatProperties = new Dictionary<string, string>();
            PropertyInfo[] properties = GetProperties<T>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].MemberType == MemberTypes.Property)
                {
                    string key = properties[i].Name;
                    if (properties[i].GetValue(data) != null)
                    {
                        object value = properties[i].GetValue(data);
                        if (value.GetType().IsPrimitive || value.GetType() == typeof(string))
                        {
                            flatProperties.Add(key, properties[i].GetValue(data).ToString());
                        }
                        else
                        {
                            Dictionary<string, string> props = Flatten(value);
                            foreach (var prop in props)
                            {
                                flatProperties.Add(prop.Key, prop.Value);
                            }
                        }
                    }
                }
            }
            return flatProperties;
        }

        private static PropertyInfo[] GetProperties<T>() where T : class
        {
            return typeof(T).GetProperties();
        }
    }
}
