using BusinessLayer.Controllers;
using BusinessLayer.ProcessOptions;
using BusinessLayer.ProcessOptions.Enums;
using BusinessLayer.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;
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
        private PowerController controller;
        private bool _buttonsEnabled = true;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                _buttonsEnabled = value;
                OnPropertyChanged();
            }
        }

        public PowerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void DisposeAllResources()
        {
            timer.Close();
            timer.Dispose();
            controller?.ClearOptions();
            CancelProgressBarAnimation();
        }

        private double GetTextBoxValue()
        {
            if (IsInitialized && InputTextBox.Text != null)
            {
                if (double.TryParse(InputTextBox.Text, out double value))
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

        private void ExecuteController(List<PowerOptions> options)
        {
            controller = new PowerController()
            {
                StartOptions = new StartOptions<PowerOptions>()
                {
                    Options = options,
                    Translater = new PowerEnumTranslater()
                },
            };
            controller.Execute();
        }

        private void StartTimer(ElapsedEventHandler function)
        {
            try
            {
                double delay = GetTextBoxValue();
                Duration duration = GetDurationFromComboBox(delay);

                timer = new Timer(duration.TimeSpan.TotalMilliseconds)
                {
                    AutoReset = false,
                };
                timer.Elapsed += function;

                DoubleAnimation animation = new DoubleAnimation(100.0, duration);
                animation.Completed += Animation_Completed;
                TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);

                timer.Start();
                ButtonsEnabled = false;
            }
            catch (FormatException)
            {
                // TODO : Show alert window
            }
        }

        private void CancelProgressBarAnimation() => TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);


        #region power management methods
        private void Shutdown(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();

            ExecuteController(new List<PowerOptions>() { PowerOptions.Shutdown, PowerOptions.NoDelay });
        }

        private void Restart(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();

            ExecuteController(new List<PowerOptions>() { PowerOptions.Restart, PowerOptions.NoDelay });
        }

        private void Lock(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();

            ExecuteController(new List<PowerOptions>() { PowerOptions.LogOff, PowerOptions.Force });
        }

        private void Sleep(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            timer.Close();

            ExecuteController(new List<PowerOptions>() { PowerOptions.Hibernate, PowerOptions.NoDelay });
        }
        #endregion

        #region events

        #region button events
        private void ShutdownButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer(Shutdown);
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer(Restart);
        }

        private void LockButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer(Lock);
        }

        private void SleepButton_Click(object sender, RoutedEventArgs e)
        {
            StartTimer(Sleep);
        }

        #endregion

        #region timer handling
        private void RestartTimer_Click(object sender, RoutedEventArgs e)
        {
            StopTimer_Click(sender, e);
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
                CancelProgressBarAnimation();
                ButtonsEnabled = true;
            }
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            // TODO : dispose all resources
            DisposeAllResources();
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
        }

        private void InputTextBlock_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            InputTextBox.Text = string.Empty;
        }

        #endregion

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
