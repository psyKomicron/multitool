using Multitool.Controllers;
using Multitool.ProcessOptions;
using Multitool.ProcessOptions.Enums;
using Multitool.ProcessOptions.EnumTranslaters;

using MultitoolWPF.Tools;
using MultitoolWPF.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for PowerWindow.xaml
    /// </summary>
    public partial class PowerWindow : ISerializableWindow, INotifyPropertyChanged
    {
        private const string inputDelay = "Input delay";
        private readonly Regex timespanRegex = new Regex(@"([0-9]+:[0-5][0-9]:[0-5][0-9])", RegexOptions.Compiled);
        private readonly Regex inputTextBoxRegex = new Regex(@"([0-9])+", RegexOptions.Compiled);
        private bool _buttonsEnabled = true;
        private Timer timer;
        private PowerController controller;
        private ElapsedEventHandler timerHandler;

        public PowerWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                _buttonsEnabled = value;
                Tool.FirePropertyChangedEvent(PropertyChanged, this);
            }
        }

        public PowerWindowData Data { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }

        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowData<PowerWindowData>(Name);
            if (Data == null)
            {
                Data = new PowerWindowData();
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
            // conditional expr are great but i want to be able to read my code ;)
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
                TimerProgressBar.BeginAnimation(RangeBase.ValueProperty, animation, HandoffBehavior.SnapshotAndReplace);

                timer.Start();
                ButtonsEnabled = false;
            }
            catch (FormatException e)
            {
                _ = MessageBox.Show(e.Message);
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
            ExecuteCommand(new List<PowerOptions>()
            {
                PowerOptions.Shutdown,
                PowerOptions.NoDelay
            });
        }

        private void Restart(object sender, ElapsedEventArgs e)
        {
            ExecuteCommand(new List<PowerOptions>()
            {
                PowerOptions.Restart,
                PowerOptions.NoDelay
            });
        }

        private void Lock(object sender, ElapsedEventArgs e)
        {
            ExecuteCommand(new List<PowerOptions>()
            {
                PowerOptions.LogOff,
                PowerOptions.Force
            });
        }

        private void Sleep(object sender, ElapsedEventArgs e)
        {
            ExecuteCommand(new List<PowerOptions>()
            {
                PowerOptions.Hibernate,
                PowerOptions.NoDelay
            });
        }

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

        private void InputTextBlock_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            InputTextBox.Text = inputDelay;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    TypeComboBox.SelectedIndex = 0;
                }
                else if (inputTextBoxRegex.Match(textBox.Text).Success)
                {
                    if (textBox.Text.Contains(":"))
                    {
                        string seconds;
                        string hours = string.Empty, minutes = string.Empty;
                        string[] s = textBox.Text.Split(':');
                        switch (s.Length)
                        {
                            case 1:
                                seconds = s[0];
                                break;
                            case 2:
                                minutes = s[0];
                                seconds = s[1];
                                break;
                            case 3:
                                hours = s[0];
                                minutes = s[1];
                                seconds = s[2];
                                break;
                            default:
                                throw new FormatException();
                        }

                        Debug.WriteLine(hours + ":" + minutes + ":" + seconds);
                    }
                    //double value = double.Parse(textBox.Text);

                }
                e.Handled = true;
            }
        }
        #endregion

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

        private void GeneralOptions_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MultiToolWindowChrome_CloseClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void MultiToolWindowChrome_MinimizeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            WindowState = WindowState.Minimized;
        }

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
