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
            Console.WriteLine("Preference file creation/writing time : " + stopwatch.ElapsedMilliseconds);
        }
    }
}
