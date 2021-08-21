using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Multitool.PerformanceMonitors
{
    /// <summary>
    /// <see cref="PerformanceMonitor"/> with asynchrone construction.
    /// </summary>
    public class AsyncPerformanceMonitor : PerformanceMonitor
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="category">Performance counter category</param>
        /// <param name="eventInterval">Interval between each event (in ms)</param>
        public AsyncPerformanceMonitor(MonitorCategory category, double eventInterval) : base(category)
        {
            Debug.WriteLine(GetHashCode().ToString() + " -> AsyncPerformanceMonitor.Ctor: " + category.ToString());
            EventInterval = eventInterval;
            CancellationTokenSource = new CancellationTokenSource();
            Debug.WriteLine(GetHashCode().ToString() + " -> Starting task for: " + category.ToString());
            InstanciationTask = Task.Run(CreatePerfCounter, CancellationTokenSource.Token);
        }

        /// <summary>
        /// The <see cref="Task"/> associated with this object instanciation.
        /// </summary>
        public Task InstanciationTask { get; }

        /// <summary>
        /// The cancellation token of the instanciation task.
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <inheritdoc/>
        public override void Dispose()
        {
            if (!Disposed)
            {
                try
                {
                    CancellationTokenSource.Cancel();
                    InstanciationTask.Wait();
                }
                catch (OperationCanceledException) { }
                catch (AggregateException e)
                {
                    Trace.WriteLine(e.ToString());
                }

                base.Dispose();
            }
        }

        private void CreatePerfCounter()
        {
            CancellationTokenSource.Token.ThrowIfCancellationRequested();
            PerformanceCounter = PerformanceMonitorFactory.Create(Category);
            CancellationTokenSource.Token.ThrowIfCancellationRequested();
            Debug.WriteLine(GetHashCode().ToString() + " -> Finished perf. mon. creation");
        }
    }
}
