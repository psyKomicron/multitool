using BusinessLayer.PreferencesManager;
using System;
using System.Diagnostics;
using System.Windows;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("Application exiting... Saving preferences : \n");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PreferenceManager manager = Tool.GetPreferenceManager();
            manager.SavePreferences();
            stopwatch.Stop();

            Console.WriteLine("\tPreference file creation/writing time : " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine(new string('-', 80));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Tool.SetPreferenceManagerPath("C:\\Users\\julie\\Documents\\MultiTool\\preferences\\userpreferences.json");
            Tool.GetPreferenceManager().DeserializePreferenceManager();
        }
    }
}
