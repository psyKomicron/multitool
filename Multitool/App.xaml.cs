using BusinessLayer.Reflection;
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
        public IPropertyLoader PropertyLoader { get; set; }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SerializeApplication();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Tool.SetPreferenceManagerPath("C:\\Users\\julie\\Documents\\MultiTool\\preferences\\userpreferences.xml");
            Tool.GetPreferenceManager().DeserializePreferenceManager();
            PropertyLoader = new PropertyLoader();
        }

        private void SerializeApplication()
        {
            Console.WriteLine(new string('-', 80));
            Console.WriteLine("Application exiting... Saving preferences : \n");

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            /* timed instructions here */

            var tool = Tool.GetPreferenceManager();
            tool.SerializePreferenceManager();

            /* end timed instructions */
            stopwatch.Stop();

            Console.WriteLine("\tPreference file creation/writing time : " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine(new string('-', 80));
        }
    }
}
