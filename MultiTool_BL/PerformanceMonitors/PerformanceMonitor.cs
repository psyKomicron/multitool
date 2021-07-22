using Multitool.Optimisation;

using System.Diagnostics;
using System.Timers;

namespace Multitool.PerformanceMonitors
{
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private PerformanceCounter performanceCounter;
        private Timer timer = new Timer(10);
        private CircularBag<float> buffer = new CircularBag<float>(10);
        private float lastAvg;

        public PerformanceMonitor(MonitorCategory category)
        {
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            performanceCounter = PerformanceCounterFactory.CreatePerformanceCounter(category);
            timer.Start();
        }

        protected PerformanceMonitor() 
        {
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
        }

        protected PerformanceCounter PerformanceCounter
        {
            get => performanceCounter;
            set => performanceCounter = value;
        }
        protected Timer Timer => timer;

        public float GetStats()
        {
            timer.Stop();
            float avg = 0;
            for (int i = 0; i < 10; i++)
            {
                avg += buffer[i];
            }
            avg /= buffer.Length;
            lastAvg = (avg + lastAvg) / 2;
            timer.Start();
            return lastAvg;
        }

        public virtual void Dispose()
        {
            timer.Stop();
            timer.Close();
            timer.Dispose();

            if (performanceCounter != null)
            {
                performanceCounter.Dispose();
            }
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            buffer.Add(performanceCounter.NextValue());
            timer.Start();
        }
    }
}
