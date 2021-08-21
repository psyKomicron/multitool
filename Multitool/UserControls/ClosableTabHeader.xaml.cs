using System.Windows;
using System.Windows.Controls;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for ClosableTabItem.xaml
    /// </summary>
    public partial class ClosableTabHeader : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ClosableTabHeader));
        public static readonly RoutedEvent CloseEvent = EventManager.RegisterRoutedEvent(nameof(Close), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ClosableTabHeader));

        public ClosableTabHeader(TabItem parent)
        {
            InitializeComponent();
            TabItem = parent;
        }

        public TabItem TabItem { get; }
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public event RoutedEventHandler Close
        {
            add
            {
                AddHandler(CloseEvent, value);
            }
            remove
            {
                RemoveHandler(CloseEvent, value);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(CloseEvent, this));
        }
    }
}
