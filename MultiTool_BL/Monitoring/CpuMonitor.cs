using Multitool.Optimisation;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;

namespace Multitool.Monitoring
{
    public class CpuMonitor : IDisposable
    {
        private PerformanceCounter performanceCounter;
        private float lastAvg;
        private Timer timer = new Timer(10);
        private CircularBag<float> buffer = new CircularBag<float>(10);

        public CpuMonitor()
        {
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            _ = Task.Run(() => PerfCounterCallback());
        }

        public bool Ready { get; private set; }

        public void Dispose()
        {
            if (performanceCounter != null)
            {
                performanceCounter.Dispose();
            }
            
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
            }
        }

        public float GetCpuUsage()
        {
            timer.Stop();
            float avg = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                avg += buffer[i];
            }
            avg /= buffer.Length;
            lastAvg = (avg + lastAvg) / 2;
            timer.Start();
            return lastAvg;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            buffer.Add(performanceCounter.NextValue());
            timer.Start();
        }

        private void PerfCounterCallback()
        {
            performanceCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            timer.Start();
            Ready = true;
        }
    }
}
