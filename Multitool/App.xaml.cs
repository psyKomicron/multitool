using Multitool.PreferencesManagers;

using MultitoolWPF.Tools;

using System;
using System.Diagnostics;
using System.Windows;

namespace Multitool
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
            Trace.WriteLine("Starting application...");
            WindowManager.InitializePreferenceManager("C:\\Users\\julie\\Documents\\MultiTool\\preferences\\userpreferences.xml");
            WindowManager.PreferenceManager.DeserializePreferenceManager();
        }

        private void SerializeApplication()
        {
#if TRACE
            Trace.WriteLine(new string('-', 80));
            Trace.WriteLine("Application exiting... Saving preferences : \n");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            /* timed instructions here */
#endif

            IPreferenceManager tool = WindowManager.PreferenceManager;
            tool.SerializePreferenceManager();

#if TRACE
            /* end timed instructions */
            stopwatch.Stop();

            Trace.WriteLine("\tPreference file creation/writing time : " + (stopwatch.ElapsedTicks * (TimeSpan.TicksPerMillisecond / 10 ^ 6)) + " ns");
            Trace.WriteLine(new string('-', 80));
#endif
        }
    }
}
