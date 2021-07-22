using System;
using System.Threading;
using System.Threading.Tasks;

namespace Multitool.PerformanceMonitors
{
    public class AsyncPerformanceMonitor : PerformanceMonitor
    {
        private Task instanciationTask;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private MonitorCategory monitorCategory;

        public AsyncPerformanceMonitor(MonitorCategory category) : base()
        {
            monitorCategory = category;
            instanciationTask = Task.Run(CreatePerfCounter, cancellationTokenSource.Token);
        }

        public Task InstanciationTask => instanciationTask;

        public override void Dispose()
        {
            cancellationTokenSource.Cancel();
            try
            {
                instanciationTask.Wait();
            }
            catch (OperationCanceledException) { }
            catch (AggregateException e)
            {
                _ = Console.Out.WriteLineAsync(e.ToString());
            }

            base.Dispose();
        }

        private void CreatePerfCounter()
        {
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            PerformanceCounter = PerformanceCounterFactory.CreatePerformanceCounter(monitorCategory);
            cancellationTokenSource.Token.ThrowIfCancellationRequested();
            Timer.Start();
        }
    }
}
