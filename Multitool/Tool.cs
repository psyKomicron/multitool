using BusinessLayer.PreferencesManager;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MultiTool
{
    internal static class Tool
    {
        private static PreferenceManager preferenceManager;

        public static PreferenceManager GetPreferenceManager() => preferenceManager;

        public static void SetPreferenceManagerPath(string path)
        {
            if (preferenceManager != null)
            {
                preferenceManager.Path = path;
            }
            else
            {
                preferenceManager = new PreferenceManager(path);
            }
        }

        public static void FireEvent(PropertyChangedEventHandler propertyChanged, object source, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(source, new PropertyChangedEventArgs(name));
        }

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
                        if ()
                        flatProperties.Add(key, properties[i].GetValue(data).ToString());
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
