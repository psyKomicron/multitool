using BusinessLayer;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for PowerWindow.xaml
    /// </summary>
    public partial class PowerWindow : Window, INotifyPropertyChanged
    {
        private Timer timer;
        private PowerController powerController;

        public bool CountingDown => timer == null;

        public event PropertyChangedEventHandler PropertyChanged;

        public PowerWindow()
        {
            InitializeComponent();
            DataContext = this;
            powerController = new PowerController();
        }

        private double GetTextBoxValue()
        {
            if (IsInitialized && InputTextBox.Text != null)
            {
                double value = -1;
                if (double.TryParse(InputTextBox.Text, out value))
                {
                    return value;
                }
                else
                {
                    throw new FormatException("Delay input is not valid (could not be converted into a number)");
                }
            }
            else
            {
                throw new FormatException("Please input a delay");
            }
        }

        private Duration GetDurationFromComboBox(double delay)
        {
            string value = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (value)
            {
                case "seconds":
                    return new Duration(TimeSpan.FromSeconds(delay));
                case "minutes":
                    return new Duration(TimeSpan.FromMinutes(delay));
                case "hours":
                    return new Duration(TimeSpan.FromHours(delay));
                default:
                    throw new Exception();
            }
        }

        private void StartTimer()
        {
            try
            {
                double delay = GetTextBoxValue();
                Duration duration = GetDurationFromComboBox(delay);

                timer = new Timer(duration.TimeSpan.TotalMilliseconds);
                timer.AutoReset = false;
                timer.Elapsed += Shutdown;

                DoubleAnimation animation = new DoubleAnimation(100.0, duration);
                animation.Completed += Animation_Completed;
                TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);

                timer.Start();
                OnPropertyChanged("CountingDown");
            }
            catch (FormatException)
            {
                // TODO : Show alert window
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region power management methods
        private void Shutdown(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();
            powerController.Shutdown();
        }

        private void Restart(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();
            powerController.Restart();
        }

        private void Lock(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();
            powerController.Lock();
        }

        private void Sleep(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();
            powerController.Sleep();
        }
        #endregion

        #region events

        private void Animation_Completed(object sender, EventArgs e)
        {
            TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
        }

        private void InputTextBlock_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            InputTextBox.Text = string.Empty;
        }

        #region button events
        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SleepButton_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region timer handling
        private void RestartTimer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
                timer = null;
                OnPropertyChanged("CountingDown");
                TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
            }
        }
        #endregion

        #endregion

    }
}
