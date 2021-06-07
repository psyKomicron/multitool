using Multitool.Sorting;
using MultitoolWPF.ViewModels;

using MultitoolWPF.Tools;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for Spreadsheet.xaml
    /// </summary>
    public partial class SpreadsheetWindow : Window, ISerializableWindow
    {
        public SpreadsheetWindow()
        {
            InitializeComponent();
            DataContext = this;
            DatePicker.SelectedDate = DateTime.Now;
        }

        public SpreadsheetWindowData Data { get; set; }

        #region ISerializableWindow
        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowData<SpreadsheetWindowData>(Name);
        }

        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }
        #endregion

        private void DisplayNonValid(ValidationCodes code)
        {
            if ((code & ValidationCodes.EmptyName) == ValidationCodes.EmptyName)
            {
                NameTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                NameTextBox.BorderBrush = new SolidColorBrush(Colors.White);
            }

            if ((code & ValidationCodes.MalformedDate) == ValidationCodes.MalformedDate)
            {
                DatePicker.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                DatePicker.BorderBrush = new SolidColorBrush(Colors.White);
            }

            if ((code & ValidationCodes.EmptyRanking) == ValidationCodes.EmptyRanking)
            {
                RankingTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
            }
            else
            {
                RankingTextBox.BorderBrush = new SolidColorBrush(Colors.White);
            }
        }

        private async Task SortEntries()
        {
            await Task.Run(() =>
            {
                SpreadsheetVM[] spreadsheetVMs = ObservableCollectionQuickSort.Sort(Data.Items);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Data.Items.Clear();

                    for (int i = spreadsheetVMs.Length - 1; i >= 0; i--)
                    {
                        Data.Items.Add(spreadsheetVMs[i]);
                    }
                });
            });
        }

        #region events handlers
        #region window & window chrome
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
        #endregion

        private void InputTextBox_1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int ranking = 0;
                string name = NameTextBox.Text;
                DateTime? date = DatePicker.SelectedDate;
                ValidationCodes validationCode = ValidationCodes.Validated;

                if (string.IsNullOrWhiteSpace(name))
                {
                    validationCode |= ValidationCodes.EmptyName;
                }
                if (string.IsNullOrWhiteSpace(RankingTextBox.Text) || !int.TryParse(RankingTextBox.Text, out ranking))
                {
                    validationCode |= ValidationCodes.EmptyRanking;
                }
                if (date == null)
                {
                    validationCode |= ValidationCodes.MalformedDate;
                }


                if (validationCode == ValidationCodes.Validated)
                {
                    Data.Items.Add(new SpreadsheetVM()
                    {
                        Name = name,
                        Date = date ?? DateTime.Now,
                        Ranking = ranking
                    });

                    _ = SortEntries();
                }
                else
                {
                    DisplayNonValid(validationCode);
                }
            }
        }

        private void MenuItemClear_Click(object sender, RoutedEventArgs e)
        {
            Data.Items.Clear();
        }
        #endregion
    }

    enum ValidationCodes
    {
        EmptyName = 0b1,
        EmptyRanking = 0b10,
        MalformedDate = 0b100,
        Validated = 0b0
    }
}
