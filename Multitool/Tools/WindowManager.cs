using Multitool.PreferencesManagers;
using Multitool.PreferencesManagers.Xml;

using MultitoolWPF.Windows;

using System;
using System.Reflection;
using System.Windows;

namespace MultitoolWPF.Tools
{
    public static class WindowManager
    {
        public static void InitializePreferenceManager(string path)
        {
            if (PreferenceManager == null)
            {
                PreferenceManager = new XmlPreferenceManager() { Path = path };
            }
        }

        public static IPreferenceManager PreferenceManager { get; private set; }
        public static MainWindow MainWindow { get; set; }

        /// <summary>
        /// Opens a window of the <typeparamref name="WindowType"/> type if a window of <typeparamref name="WindowType"/>
        /// is not already openened.
        /// </summary>
        /// <typeparam name="WindowType">Type of the window to open</typeparam>
        /// <returns>Returns true if the <see cref="Window"/> was not open and thus has been created, false if the 
        /// <see cref="Window"/> was already opened and thus was not created.</returns>
        public static bool Open<WindowType>(params object[] ctorParams) where WindowType : Window, ISerializableWindow, new()
        {
            if (Application.Current.Windows.Count > 0)
            {
                Window instance = null;

                for (int i = 0; i < Application.Current.Windows.Count; i++)
                {
                    if (Application.Current.Windows[i].GetType() == typeof(WindowType))
                    {
                        instance = Application.Current.Windows[i];
                        break;
                    }
                }

                if (instance != null)
                {
                    instance.Activate();
                    return false;
                }
                else
                {
                    CreateAndOpen<WindowType>();
                    return true;
                }
            }
            else
            {
                CreateAndOpen<WindowType>();
                return true;
            }
        }

        public static bool Open(Type windowType)
        {
            if (Application.Current.Windows.Count > 0)
            {
                Window instance = null;

                for (int i = 0; i < Application.Current.Windows.Count; i++)
                {
                    if (Application.Current.Windows[i].GetType() == windowType)
                    {
                        instance = Application.Current.Windows[i];
                        break;
                    }
                }

                if (instance != null)
                {
                    _ = instance.Activate();
                    return false;
                }
                else
                {
                    CreateAndOpen(windowType);
                    return true;
                }
            }
            else
            {
                CreateAndOpen(windowType);
                return true;
            }
        }

        private static void CreateAndOpen<WindowType>() where WindowType : Window, ISerializableWindow, new()
        {
            WindowType w = new WindowType();
            w.Closing += Window_Closing;
            w.Deserialize();
            w.Show();
        }

        private static void CreateAndOpen(Type windowType)
        {
            ConstructorInfo cInfo = windowType.GetConstructor(new Type[0]);
            if (cInfo == null)
            {
                throw new ArgumentException(windowType.Name + " does not have a parameter-less constructor");
            }
            object o = cInfo.Invoke(new Type[0]);
            if (typeof(Window).IsAssignableFrom(o.GetType()) && o is ISerializableWindow serializableWindow)
            {
                Window w = (Window)o;
                w.Closing += Window_Closing;
                serializableWindow.Deserialize();
                w.Show();
            }
        }

        #region events

        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is ISerializableWindow serializableWindow)
            {
                serializableWindow.Serialize();
                bool last = true;
                foreach (Window window in Application.Current.Windows)
                {
                    // Microsoft.VisualStudio.DesignTools.WpfTap.WpfVisualTreeService.Adorners.AdornerWindow
                    if (window != sender && window.GetType().Name != "AdornerWindow" && window.GetType() != typeof(MainWindow))
                    {
                        last = false;
                        break;
                    }
                }
                if (last)
                {
                    MainWindow.Data.StartWindow = string.Empty;
                }
            }
        }

        #endregion
    }
}
