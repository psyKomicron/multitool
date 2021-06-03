using System.Windows;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for ControlPanelsWindow.xaml
    /// </summary>
    public partial class ControlPanelsWindow : Window
    {
        public ControlPanelsWindow()
        {
            InitializeComponent();
        }

        private void MultitoolWindowChrome_CloseClick(object sender, RoutedEventArgs e) => Close();

        private void MultitoolWindowChrome_MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MultitoolWindowChrome_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }

            
        }
    }
}
