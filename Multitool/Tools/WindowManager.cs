using Multitool.PreferencesManagers;
using Multitool.PreferencesManagers.Xml;
using Multitool.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Multitool.Tools
{
    public static class WindowManager
    {
        private static IPreferenceManager preferenceManager;
        
        public static void InitializePreferenceManager(string path)
        {
            if (preferenceManager == null)
            {
                preferenceManager = new XmlPreferenceManager() { Path = path };
            }
        }

        public static IPreferenceManager PreferenceManager => preferenceManager;

        /// <summary>
        /// Opens a window of the <typeparamref name="WindowType"/> type if a window of <typeparamref name="WindowType"/>
        /// is not already openened.
        /// </summary>
        /// <typeparam name="WindowType">Type of the window to open</typeparam>
        /// <returns>Returns true if the <see cref="Window"/> was not open and thus has been created, false if the 
        /// <see cref="Window"/> was already opened and thus was not created.</returns>
        public static bool Open<WindowType>() where WindowType : Window, ISerializableWindow, new()
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

        private static void CreateAndOpen<WindowType>() where WindowType : Window, ISerializableWindow, new()
        {
            WindowType w = new WindowType();

            w.Closing += Window_Closing;

            w.Deserialize();
            w.Show();
        }

        #region events

        private static void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender is ISerializableWindow serializableWindow)
            {
                serializableWindow.Serialize();
            }
        }

        #endregion
    }
}
