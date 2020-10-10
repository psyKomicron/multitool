using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for ClockWindow.xaml
    /// </summary>
    public partial class ClockWindow : Window
    {
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;
        private double _value = 0;

        public ClockWindow()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += _timer_Tick;

            _stopwatch = new Stopwatch();

            _stopwatch.Start();
            _timer.Start();
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            _value += _stopwatch.Elapsed.TotalMilliseconds;
            clockLabel.Content = _value.ToString();
            //clockLabel.
            _stopwatch.Reset();
            _stopwatch.Start();
        }
    }
}
