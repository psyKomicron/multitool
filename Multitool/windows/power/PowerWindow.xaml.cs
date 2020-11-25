using BusinessLayer.Controllers;
using BusinessLayer.PreferencesManagers;
using BusinessLayer.ProcessOptions;
using BusinessLayer.ProcessOptions.Enums;
using BusinessLayer.ProcessOptions.EnumTranslaters;
using MultiTool.Windows;
using MultiTool.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using BusinessLayer.PreferencesManagers.Xml;
using MultiTool.Tools;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for PowerWindow.xaml
    /// </summary>
    public partial class PowerWindow : ISerializableWindow, INotifyPropertyChanged
    {
        private readonly Regex timespanRegex = new Regex(@"([0-9]+:[0-5][0-9]:[0-5][0-9])");
        private readonly Regex inputTextBoxRegex = new Regex(@"([0-9])+");
        private bool _buttonsEnabled = true;
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

        public PowerWindowDTO Data { get; set; }

        public PowerWindow()
        {
            InitializeComponent();
            DataContext = this;
            Data = new PowerWindowDTO();
            Deserialize();
        }

        public void Serialize()
        {
            Dictionary<string, string> properties = Tool.Flatten(Data);

            WindowManager.GetPreferenceManager()
                .AddWindowManager(new WindowPreferenceManager() 
                { 
                    ItemName = Name, 
                    Properties = properties 
                });
        }

        public void Deserialize()
        {
            Data = WindowManager.GetPreferenceManager().GetWindowManager<PowerWindowDTO>(Name);
            if (Data == null)
            {
                Data = new PowerWindowDTO();
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
        }

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

        #region hmi methods

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
        private void Shutdown(object sender, ElapsedEventArgs e) => ExecuteController(new List<PowerOptions>() { PowerOptions.Shutdown, PowerOptions.NoDelay });

        private void Restart(object sender, ElapsedEventArgs e) => ExecuteController(new List<PowerOptions>() { PowerOptions.Restart, PowerOptions.NoDelay });

        private void Lock(object sender, ElapsedEventArgs e) => ExecuteController(new List<PowerOptions>() { PowerOptions.LogOff, PowerOptions.Force });

        private void Sleep(object sender, ElapsedEventArgs e) => ExecuteCommand(new List<PowerOptions>() { PowerOptions.Hibernate, PowerOptions.NoDelay });

        private void ExecuteCommand(List<PowerOptions> options)
        {
            timer.Stop();
            timer.Close();

            ExecuteController(options);
        }

        #endregion

        #region events

        #region other events
        private void Window_Closed(object sender, EventArgs e)
        {
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

        #endregion
    }
}
