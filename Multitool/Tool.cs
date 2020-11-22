using BusinessLayer.PreferencesManagers;
using BusinessLayer.PreferencesManagers.Json;
using BusinessLayer.PreferencesManagers.Xml;
using BusinessLayer.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MultiTool
{
    internal static class Tool
    {
        private static IPreferenceManager<XmlWindowPreferenceManager> preferenceManager;

        public static IPreferenceManager<XmlWindowPreferenceManager> GetPreferenceManager() => preferenceManager;

        public static void SetPreferenceManagerPath(string path)
        {
            if (preferenceManager != null)
            {
                preferenceManager.Path = path;
            }
            else
            {
                preferenceManager = new XmlPreferenceManager() { Path = path };
            }
        }

        public static void FireEvent(PropertyChangedEventHandler propertyChanged, object source, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(source, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// ToString but for properties of an object. Only flatten if the property is a primitive type or a string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, string> Flatten<T>(T data) where T : class
        {
            return new BasicObjectFlattener().Flatten(data);
        }

        private static PropertyInfo[] GetProperties<T>() where T : class
        {
            return typeof(T).GetProperties();
        }
    }
}
