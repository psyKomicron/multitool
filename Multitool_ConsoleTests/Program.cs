using Multitool.PerformanceMonitors;

using System;

namespace Multitool_ConsoleTests
{
    public class Program
    {
        internal static object _consoleLock = new object();

        public static void Main()
        {
            try
            {
                Console.WriteLine("Starting...");
                IPerformanceMonitor perfMon = new AsyncPerformanceMonitor(MonitorCategory.ProcessorUsageTotal, 500);
                perfMon.ValueChanged += PerfMon_ValueChanged;
                IPerformanceMonitor perfMon2 = new AsyncPerformanceMonitor(MonitorCategory.MemoryGlobal, 2500);
                perfMon2.ValueChanged += PerfMon2_ValueChanged;

                Console.WriteLine("Processor: ");
                Console.WriteLine("Memory: ");

                perfMon.Start();
                perfMon2.Start();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.ToString());
                Console.ResetColor();
            }
            _ = Console.ReadLine();
        }

        private static void PerfMon2_ValueChanged(IPerformanceMonitor sender, float value)
        {
            lock (_consoleLock)
            {
                Console.SetCursorPosition(8, 2);
                Console.Write(value);
            }
        }

        private static void PerfMon_ValueChanged(IPerformanceMonitor sender, float value)
        {
            lock (_consoleLock)
            {
                Console.SetCursorPosition(11, 1);
                Console.Write(value);
            }
        }
    }
}
