
using MultitoolWPF.Properties;

using System;
using System.ComponentModel;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;

namespace MultitoolWPF.Tools
{
    internal static class Tool
    {
        public static void FirePropertyChangedEvent(PropertyChangedEventHandler propertyChanged, object source, [CallerMemberName] string name = null)
        {
            propertyChanged?.Invoke(source, new PropertyChangedEventArgs(name));
        }

        public static ResourceManager ResourceManager => Resources.ResourceManager;
        public static Settings Settings => MultitoolWPF.Properties.Settings.Default;

        public static string GetStringResource(string name)
        {
            return Resources.ResourceManager.GetString(name);
        }

        public static T GetAppRessource<T>(string name)
        {
            ResourceDictionary dic = Application.Current.Resources;
            if (dic.Contains(name))
            {
                object o = dic[name];
                if (o.GetType() == typeof(T))
                {
                    return (T)dic[name];
                }
                else
                {
                    throw new InvalidCastException(o.GetType().ToString() + " cannot be directly casted to " + typeof(T).ToString());
                }
            }
            else
            {
                throw new ArgumentException("Application resources doesn't contains " + name);
            }
        }

        public static string FormatSize(long size)
        {
            if (size >= (long)Sizes.TERA)
            {
                return Math.Round(size / (double)Sizes.TERA, 3) + " Tb";
            }
            if (size >= (long)Sizes.GIGA)
            {
                return Math.Round(size / (double)Sizes.GIGA, 3) + " Gb";
            }
            if (size >= (long)Sizes.MEGA)
            {
                return Math.Round(size / (double)Sizes.MEGA, 3) + " Mb";
            }
            if (size >= (long)Sizes.KILO)
            {
                return Math.Round(size / (double)Sizes.KILO, 3) + " Kb";
            }
            return size + " b";
        }

        public static void FormatSize(long size, out double formatted, out string ext)
        {
            if (size >= (long)Sizes.TERA)
            {
                formatted = Math.Round(size / (double)Sizes.TERA, 3);
                ext = " Tb";
            }
            else if (size >= (long)Sizes.GIGA)
            {
                formatted = Math.Round(size / (double)Sizes.GIGA, 3);
                ext = " Gb";
            }
            else if (size >= (long)Sizes.MEGA)
            {
                formatted = Math.Round(size / (double)Sizes.MEGA, 3);
                ext = " Mb";
            }
            else if (size >= (long)Sizes.KILO)
            {
                formatted = Math.Round(size / (double)Sizes.KILO, 3);
                ext = " Kb";
            }
            else
            {
                formatted = size;
                ext = " b";
            }
        }

        private enum Sizes : long
        {
            TERA = 1000000000000,
            GIGA = 1000000000,
            MEGA = 1000000,
            KILO = 1000
        }
    }
}
