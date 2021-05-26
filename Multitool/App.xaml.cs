using Multitool.PreferencesManagers;

using MultiTool.Tools;

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
            SerializeApplication();
            e.ApplicationExitCode = 0;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Console.WriteLine("Starting application...");

            WindowManager.InitializePreferenceManager("C:\\Users\\julie\\Documents\\MultiTool\\preferences\\userpreferences.xml");
            WindowManager.PreferenceManager.DeserializePreferenceManager();
        }

        private void SerializeApplication()
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("Application exiting... Saving preferences : \n");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            /* timed instructions here */

            IPreferenceManager tool = WindowManager.PreferenceManager;
            tool.SerializePreferenceManager();

            /* end timed instructions */
            stopwatch.Stop();

            Console.WriteLine("\tPreference file creation/writing time : " + (stopwatch.ElapsedTicks * (TimeSpan.TicksPerMillisecond / 10^6)) + " ns");
            Console.WriteLine(new string('-', 80));
        }
    }
}
