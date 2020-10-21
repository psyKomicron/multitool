using BusinessLayer.PreferencesManager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MultiTool
{
    internal static class Tool
    {
        private static PreferenceManager preferenceManager = new PreferenceManager();

        public static PreferenceManager GetPreferenceManager() => preferenceManager;

        public static void FireEvent(PropertyChangedEventHandler propertyChanged, object source, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(source, new PropertyChangedEventArgs(name));
        }

        public static Dictionary<string, string> FlattenWindow<WindowType>(WindowType window) where WindowType : Window
        {
            Dictionary<string, string> flatProperties = new Dictionary<string, string>();
            PropertyInfo[] properties = GetProperties<WindowType>();
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].MemberType == MemberTypes.Property)
                {
                    string key = properties[i].Name;
                    if (properties[i].GetValue(window) != null && !string.IsNullOrEmpty(properties[i].GetValue(window).ToString()))
                    {
                        flatProperties.Add(key, properties[i].GetValue(window).ToString());
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
