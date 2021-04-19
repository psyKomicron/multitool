using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiTool.Windows
{
    /// <summary>
    /// Interaction logic for Spreadsheet.xaml
    /// </summary>
    public partial class SpreadsheetWindow : Window, ISerializableWindow
    {
        public ObservableCollection<SpreadsheetViewModel> Items { get; private set; }

        public SpreadsheetWindow()
        {
            InitializeComponent();
            DataContext = this;
            Items = new ObservableCollection<SpreadsheetViewModel>();
        }

        public void Deserialize()
        {
            
        }

        public void Serialize()
        {
            
        }

        #region event handlers
        private void WindowChrome_MaximizeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void WindowChrome_CloseClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Close();
        }

        private void WindowChrome_MinimizeClick(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            WindowState = WindowState.Minimized;
        }

        private void WindowChrome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            Items.Clear();
        }

        private void InputTextBox_1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidationCodes validationCode = ValidateFields();
                if (validationCode == ValidationCodes.Validated)
                {
                    Items.Add(new SpreadsheetViewModel()
                    {
                        Name = InputTextBox_1.Text,
                        Ranking = InputTextBox_2.Text,
                        Date = string.IsNullOrWhiteSpace(InputTextBox_3.Text) ? DateTime.Now.ToString() : InputTextBox_3.Text
                    });
                }

                if ((validationCode & ValidationCodes.EmptyName) == ValidationCodes.EmptyName)
                {
                    InputTextBox_1.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    InputTextBox_1.BorderBrush = new SolidColorBrush(Colors.White);
                }

                if ((validationCode & ValidationCodes.EmptyRanking) == ValidationCodes.EmptyRanking)
                {
                    InputTextBox_2.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    InputTextBox_2.BorderBrush = new SolidColorBrush(Colors.White);
                }

                if ((validationCode & ValidationCodes.MalformedDate) == ValidationCodes.MalformedDate)
                {
                    InputTextBox_3.BorderBrush = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    InputTextBox_3.BorderBrush = new SolidColorBrush(Colors.White);
                }
            }
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
        #endregion

        private ValidationCodes ValidateFields()
        {
            string name = InputTextBox_1.Text;
            string ranking = InputTextBox_2.Text;
            string date = InputTextBox_3.Text;
            ValidationCodes validationCode = ValidationCodes.Validated;

            if (string.IsNullOrWhiteSpace(name))
            {
                validationCode |= ValidationCodes.EmptyName;
            }
            if (string.IsNullOrWhiteSpace(ranking))
            {
                validationCode |= ValidationCodes.EmptyRanking;
            }
            if (string.IsNullOrWhiteSpace(date))
            {
                validationCode |= ValidationCodes.MalformedDate;
            }

            return validationCode;
        }
    }

    enum ValidationCodes
    {
        EmptyName = 0b1,
        EmptyRanking = 0b10,
        MalformedDate = 0b100,
        Validated = 0b0
    }
}
