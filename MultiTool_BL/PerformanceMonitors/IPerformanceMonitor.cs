using System;

namespace Multitool.PerformanceMonitors
{
    public interface IPerformanceMonitor : IDisposable
    {
        float GetStats();
    }
}