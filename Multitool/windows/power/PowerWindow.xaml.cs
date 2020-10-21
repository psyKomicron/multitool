using BusinessLayer.Controllers;
using BusinessLayer.PreferencesManager;
using BusinessLayer.ProcessOptions;
using BusinessLayer.ProcessOptions.Enums;
using BusinessLayer.ProcessOptions.EnumTranslaters;
using MultiTool.windows.power;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
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
        private bool _buttonsEnabled = true;
        private readonly Regex timespanRegex = new Regex(@"([0-9]+:[0-5][0-9]:[0-5][0-9])");
        private readonly Regex inputTextBoxRegex = new Regex(@"([0-9])+");
        private Timer timer;
        private PowerController controller;

        private ElapsedEventHandler timerHandler;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                _buttonsEnabled = value;
                Tool.FireEvent(PropertyChanged, this);
            }
        }

        public PowerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region standard methods
        private void DisposeAllResources()
        {
            if (timer != null)
            {
                timer.Close();
                timer.Dispose();
            }
            if (controller != null)
            {
                controller.ClearOptions();
            }
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

        private Duration GetDurationFromComboBox()
        {
            string value = (TypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            switch (value)
            {
                case "seconds":
                    return new Duration(TimeSpan.FromSeconds(GetTextBoxValue()));
                case "minutes":
                    return new Duration(TimeSpan.FromMinutes(GetTextBoxValue()));
                case "hours":
                    return new Duration(TimeSpan.FromHours(GetTextBoxValue()));
                case "auto-detect":
                    if (timespanRegex.Match(InputTextBox.Text).Success)
                    {
                        return new Duration(TimeSpan.Parse(InputTextBox.Text));
                    }
                    throw new FormatException("Input is not valid");
                default:
                    throw new FormatException();
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
                Duration duration = GetDurationFromComboBox();

                timer = new Timer(duration.TimeSpan.TotalMilliseconds)
                {
                    AutoReset = false,
                };
                timer.Elapsed += function;
                timerHandler = function;

                DoubleAnimation animation = new DoubleAnimation(100.0, duration);
                animation.Completed += Animation_Completed;
                TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);

                timer.Start();
                ButtonsEnabled = false;
            }
            catch (FormatException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void CancelProgressBarAnimation()
        {
            if (IsInitialized)
            {
                TimerProgressBar.BeginAnimation(ProgressBar.ValueProperty, null);
            }
        }

        #endregion

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

        #region window buttons
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            new ParameterWindow().ShowDialog();
        }
        #endregion

        #region parameter buttons
        private void GeneralOptions_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #endregion

        #region timer handling
        private void RestartTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
                CancelProgressBarAnimation();
                StartTimer(timerHandler);
                e.Handled = true;
            }
        }

        private void StopTimer_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Close();
                CancelProgressBarAnimation();
                ButtonsEnabled = true;
                e.Handled = true;
            }
        }
        #endregion

        #region other events
        private void Window_Closed(object sender, EventArgs e)
        {
            Dictionary<string, string> properties = Tool.FlattenWindow(this);

            PreferenceManager manager = Tool.GetPreferenceManager();
            manager.AddPreferenceManager(new WindowPreferenceManager() { ItemName = "PowerWindow", Values = properties });

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

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsInitialized && sender is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    TypeComboBox.SelectedIndex = 0;
                }
                else if (inputTextBoxRegex.Match(textBox.Text).Success)
                {
                    double value = double.Parse(textBox.Text);
                    if (value < 60)
                    {
                        TypeComboBox.SelectedIndex = 2;
                    }
                    else
                    {
                        TypeComboBox.SelectedIndex = 1;
                    }
                }
                e.Handled = true;
            }
        }
        #endregion

        #endregion
    }
}
