using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for SpreadsheetItemUserControl.xaml
    /// </summary>
    public partial class SpreadsheetItemUserControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region dependency properties
        public static readonly DependencyProperty ItemNameProperty =
            DependencyProperty.Register(nameof(ItemName), typeof(string), typeof(SpreadsheetItemUserControl));

        public static readonly DependencyProperty ItemDateProperty =
            DependencyProperty.Register(nameof(ItemDate), typeof(DateTime), typeof(SpreadsheetItemUserControl));

        public static readonly DependencyProperty ItemRankingProperty =
            DependencyProperty.Register(nameof(ItemRanking), typeof(int), typeof(SpreadsheetItemUserControl));

        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(SpreadsheetItemUserControl));

        public static readonly DependencyProperty ItemPaddingProperty =
            DependencyProperty.Register(nameof(ItemPadding), typeof(Thickness), typeof(SpreadsheetItemUserControl));

        public string ItemName
        {
            get => (string)GetValue(ItemNameProperty);
            set => SetValue(ItemNameProperty, value);
        }
        public DateTime ItemDate
        {
            get => (DateTime)GetValue(ItemDateProperty);
            set => SetValue(ItemDateProperty, value);
        }
        public int ItemRanking
        {
            get => (int)GetValue(ItemRankingProperty);
            set => SetValue(ItemRankingProperty, value);
        }
        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }
        public Thickness ItemPadding
        {
            get => (Thickness)GetValue(ItemPaddingProperty);
            set => SetValue(ItemPaddingProperty, value);
        }
        #endregion

        public bool IsReadOnly { get; set; }
        public double WidthMargin { get; set; }

        public SpreadsheetItemUserControl()
        {
            InitializeComponent();
            IsReadOnly = true;
            WidthMargin = default;
        }

        private void SetSelfSize()
        {
            if (!double.IsNaN(ItemWidth)) // Width has a value
            {
                double width = ItemWidth - (WidthMargin + Padding.Left + Padding.Right);

                double left, right;
                left = Margin.Left;
                right = Margin.Right;

                double fieldSize = width / 3d;
                fieldSize = Math.Round(fieldSize, 1);

                Name_TextBox.Width = fieldSize - left;
                Date_TextBox.Width = fieldSize;
                Ranking_TextBox.Width = fieldSize - right;
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IsReadOnly = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReadOnly)));
        }

        private void MenuItem_Edit(object sender, RoutedEventArgs e)
        {
            IsReadOnly = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsReadOnly)));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) => SetSelfSize();

        private void UserControl_LayoutUpdated(object sender, EventArgs e) => SetSelfSize();
    }
}
