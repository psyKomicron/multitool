using BusinessLayer.PreferencesManagers;
using BusinessLayer.PreferencesManagers.Xml;
using BusinessLayer.Reflection.ObjectFlatteners;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MultiTool.Tools
{
    internal static class Tool
    {
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

    }
}
