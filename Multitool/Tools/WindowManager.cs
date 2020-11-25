using BusinessLayer.PreferencesManagers;
using BusinessLayer.PreferencesManagers.Xml;
using MultiTool.Windows;
using System;
using System.Collections.Generic;
using System.Windows;

namespace MultiTool.Tools
{
    public static class WindowManager
    {
        private static readonly List<Window> windows = new List<Window>(3);
        private static volatile IPreferenceManager preferenceManager;
        
        public static void InitializePreferenceManager(string path)
        {
            if (preferenceManager == null)
            {
                preferenceManager = new XmlPreferenceManager() { Path = path };
            }
        }

        public static IPreferenceManager GetPreferenceManager() => preferenceManager;

        /// <summary>
        /// Opens a window of the <typeparamref name="WindowType"/> type if a window of <typeparamref name="WindowType"/>
        /// is not already openened.
        /// </summary>
        /// <typeparam name="WindowType">Type of the window to open</typeparam>
        /// <returns>Returns true if the <see cref="Window"/> was not open and thus has been created, false if the 
        /// <see cref="Window"/> was already opened and thus was not created.</returns>
        public static bool Open<WindowType>() where WindowType : Window, new()
        {
            if (windows.Count > 0)
            {
                Window instance = windows.Find((w) => w is WindowType);
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

        private static void CreateAndOpen<WindowType>() where WindowType : Window, new()
        {
            Window w = new WindowType();
            w.Closing += Window_Closing;
            w.Closed += ChildWindow_Closed;

            w.Show();
            windows.Add(w);
        }

        #region events

        private static void ChildWindow_Closed(object sender, EventArgs e)
        {
            if (sender is Window window)
            {
                Window instanceWindow = windows.Find((w) => w.Name == window.Name);
                if (instanceWindow != null)
                {
                    windows.Remove(instanceWindow);
                }
            }
        }

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
