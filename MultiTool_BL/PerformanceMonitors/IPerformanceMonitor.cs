using System;

namespace Multitool.PerformanceMonitors
{
    /// <summary>
    /// Delegate for the <see cref="IPerformanceMonitor.ValueChanged"/> event.
    /// </summary>
    /// <param name="sender"><see cref="IPerformanceMonitor"/> raising the event</param>
    /// <param name="value">The new value</param>
    public delegate void ValueChangedEventHandler(IPerformanceMonitor sender, float value);

    /// <summary>
    /// Interface for a wrapper around <see cref="System.Diagnostics.PerformanceCounter"/>.
    /// </summary>
    public interface IPerformanceMonitor : IDisposable
    {
        /// <summary>
        /// Raised when a new value is available or has been changed.
        /// </summary>
        event ValueChangedEventHandler ValueChanged;

        /// <summary>
        /// Category of this performance monitor.
        /// </summary>
        MonitorCategory Category { get; }

        /// <summary>
        /// Interval between each "value changed" event.
        /// </summary>
        double EventInterval { get; set; }

        /// <summary>
        /// Starts the monitor.
        /// </summary>
        void Start();
    }
}