using Multitool.Optimisation;

using System;
using System.Diagnostics;
using System.Timers;

namespace Multitool.PerformanceMonitors
{
    /// <summary>
    /// 
    /// </summary>
    public class PerformanceMonitor : IPerformanceMonitor
    {
        private float lastAvg;
        private bool disposed;
        private CircularBag<float> buffer = new CircularBag<float>(10);

        #region constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="category">Category of this performance monitor</param>
        /// <param name="eventInterval">Interval at wich the <see cref="ValueChanged"/> event will be fired</param>
        public PerformanceMonitor(MonitorCategory category, double eventInterval) : this(category)
        {
            EventInterval = eventInterval;
            PerformanceCounter = PerformanceMonitorFactory.Create(category);
        }

        /// <summary>
        /// Constructor for derived classes. Only instanciates the timer with the delegate for the <see cref="Timer.Elapsed"/> event and set the <see cref="Timer.AutoReset"/> property to true. Consumer needs to set <see cref="EventInterval"/>.
        /// </summary>
        protected PerformanceMonitor(MonitorCategory category)
        {
            PollingTimer = new Timer(10)
            {
                AutoReset = true
            };
            PollingTimer.Elapsed += Timer_Elapsed;

            EventTimer = new Timer()
            {
                AutoReset = true
            };
            EventTimer.Elapsed += EventTimer_Elapsed;
        }

        #endregion

        #region properties

        /// <inheritdoc/>
        public MonitorCategory Category { get; protected set; }

        /// <summary>
        /// Timer for the <see cref="ValueChanged"/> event.
        /// </summary>
        public Timer EventTimer { get; }

        /// <inheritdoc/>
        public double EventInterval 
        {
            get => EventTimer.Interval;
            set => EventTimer.Interval = value; 
        }

        /// <summary>
        /// Timer to poll the <see cref="PerformanceCounter"/>.
        /// </summary>
        protected Timer PollingTimer { get; }

        /// <summary>
        /// The <see cref="System.Diagnostics.PerformanceCounter"/> associated with this class.
        /// </summary>
        protected PerformanceCounter PerformanceCounter { get; set; }

        /// <summary>
        /// True is the object's <see cref="Dispose"/> method has already been called.
        /// </summary>
        protected bool Disposed { get; set; }
        #endregion

        /// <inheritdoc/>
        public event ValueChangedEventHandler ValueChanged;

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposed = true;

                EventTimer.Stop();
                PollingTimer.Stop();

                EventTimer.Close();
                EventTimer.Dispose();

                PollingTimer.Close();
                PollingTimer.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Start()
        {
            Debug.WriteLine(GetHashCode() + " -> Starting performance monitor");
            PollingTimer.Start();
            EventTimer.Start();
        }

        /// <inheritdoc/>
        private float GetStats()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(PerformanceMonitor));
            }

            PollingTimer.Stop();

            float avg = 0;
            for (int i = 0; i < 10; i++)
            {
                avg += buffer[i];
            }

            avg /= buffer.Length;
            lastAvg = (avg + lastAvg) / 2;

            PollingTimer.Start();
            return lastAvg;
        }

        private void EventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ValueChanged?.Invoke(this, GetStats());
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (PerformanceCounter != null)
            {
                PollingTimer.Stop();
                buffer.Add(PerformanceCounter.NextValue());
                PollingTimer.Start();
            }
        }
    }
}
