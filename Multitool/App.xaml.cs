using BusinessLayer.PreferencesManager;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PreferenceManager manager = Tool.GetPreferenceManager();
            manager.SavePreferences();

            stopwatch.Stop();
            Console.WriteLine(new string('-', 70));
            Console.WriteLine("Preference file creation/writing time : " + stopwatch.ElapsedMilliseconds + "ms");
            Console.WriteLine(new string('-', 70));
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

        }
    }
}
