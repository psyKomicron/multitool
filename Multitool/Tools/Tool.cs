using BusinessLayer.PreferencesManagers;
using BusinessLayer.PreferencesManagers.Xml;
using BusinessLayer.Reflection.ObjectFlatteners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml;

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
            try
            {
                XmlNode xml = new XmlObjectFlattener().Flatten(data);
            }
            catch (StackOverflowException e)
            {
                Console.Error.WriteLine(e.ToString());
                Console.WriteLine("Data could not be flatten into a xml format");
            }

            try
            {
                return new BasicObjectFlattener().Flatten(data);
            }
            catch (StackOverflowException e)
            {
                Console.WriteLine(e.ToString());
            }
            return null;
        }

    }
}
