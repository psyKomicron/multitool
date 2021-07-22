using System;
using System.Diagnostics;

namespace Multitool.PerformanceMonitors
{
    public static class PerformanceCounterFactory
    {
        public static PerformanceCounter CreatePerformanceCounter(MonitorCategory category)
        {
            switch (category)
            {
                case MonitorCategory.ProcessorUsageTotal:
                    return new PerformanceCounter("Processor", "% Processor Time", "_Total");
                case MonitorCategory.MemoryGlobal:
                    return new PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", "_Global_");
                case MonitorCategory.MemoryProcess:
                    return new PerformanceCounter(".NET CLR Memory", "# Bytes in all Heaps", "Multitool");
                default:
                    throw new ArgumentException("Category not recognized");
            }
        }
    }
}
