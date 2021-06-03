using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MultitoolWPF.Windows.Explorer
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {
        private bool started;
        private Task consumerTask;
        private CancellationTokenSource cancelSource = new CancellationTokenSource();
        private Queue<Exception> exceptions = new Queue<Exception>(20);
        private object _lock = new object();
        private ExceptionTextFormatter formatter = new ExceptionTextFormatter();

        public ExceptionWindow()
        {
            InitializeComponent();
            Closed += ExceptionWindow_Closed;
        }

        public void Start()
        {
            if (consumerTask != null)
            {
                if (consumerTask.Status != TaskStatus.Running)
                {
                    started = true;
                    consumerTask = new Task(() => Consume(cancelSource.Token), cancelSource.Token, TaskCreationOptions.LongRunning);
                    consumerTask.Start();
                }
            }
            else
            {
                started = true;
                consumerTask = new Task(() => Consume(cancelSource.Token), cancelSource.Token, TaskCreationOptions.LongRunning);
                consumerTask.Start();
            }
        }

        public void Stop()
        {
            if (consumerTask.Status == TaskStatus.Running)
            {
                cancelSource.Cancel();
                
            }
        }

        public void Queue(Exception e)
        {
            lock (_lock)
            {
                exceptions.Enqueue(e);
            }
        }

        private void Consume(CancellationToken token)
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (exceptions.Count > 0)
                {
                    for (int i = 0; i < exceptions.Count; i++)
                    {
                        token.ThrowIfCancellationRequested();
                        Exception current;
                        lock (_lock)
                        {
                            current = exceptions.Dequeue();
                        }

                        SendToDisplay(current);
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void SendToDisplay(Exception message)
        {
            Dispatcher.Invoke(() =>
            {
                List<Run> results = formatter.GetFormatting(message);
                foreach (Run result in results)
                {
                    Exceptions_TextBlock.Inlines.Add(result);
                }
                ScrollViewer.ScrollToBottom();
            });
        }

        private void ExceptionWindow_Closed(object sender, EventArgs e)
        {
            cancelSource.Cancel();
        }
    }
}
