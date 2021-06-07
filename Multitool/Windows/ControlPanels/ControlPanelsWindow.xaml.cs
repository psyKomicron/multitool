using MultitoolWPF.Tools;
using MultitoolWPF.ViewModels;

using System.Windows;

namespace MultitoolWPF.Windows
{
    /// <summary>
    /// Interaction logic for ControlPanelsWindow.xaml
    /// </summary>
    public partial class ControlPanelsWindow : Window, ISerializableWindow
    {
        public ControlPanelsWindow()
        {
            InitializeComponent();
        }

        public DefaultWindowData Data { get; set; }

        public void Deserialize()
        {
            Data = WindowManager.PreferenceManager.GetWindowData<DefaultWindowData>(Name);
        }

        public void Serialize()
        {
            WindowManager.PreferenceManager.AddWindowData(Data, Name);
        }

        private void MultitoolWindowChrome_CloseClick(object sender, RoutedEventArgs e) => Close();

        private void MultitoolWindowChrome_MinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void MultitoolWindowChrome_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                e.Handled = true;
                DragMove();
            }
        }
    }
}
