using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultiTool
{
    /// <summary>
    /// Interaction logic for WindowChrome.xaml
    /// </summary>
    public partial class MultiToolWindowChrome : UserControl
    {
        private int closeListeners = 0;
        private int minimizeListeners = 0;

        public string Title { get; set; }

        public event RoutedEventHandler CloseClick
        {
            add
            {
                WindowCloseButton.Click += value;
                
                if (!WindowCloseButton.IsEnabled)
                {
                    WindowCloseButton.IsEnabled = true;
                }
                closeListeners++;
            }
            remove
            {
                WindowCloseButton.Click -= value;

                if (closeListeners > 0)
                {
                    closeListeners--;
                }

                if (closeListeners == 0)
                {
                    WindowCloseButton.IsEnabled = false;
                }
            }
        }

        public event RoutedEventHandler MinimizeClick
        {
            add
            {
                WindowMinimizeButton.Click += value;
                if (!WindowMinimizeButton.IsEnabled)
                {
                    WindowMinimizeButton.IsEnabled = true;
                }
                minimizeListeners++;
            }
            remove
            {
                WindowMinimizeButton.Click -= value;
               
                if (minimizeListeners > 0)
                {
                    minimizeListeners--;
                }

                if (minimizeListeners == 0)
                {
                    WindowMinimizeButton.IsEnabled = false;
                }
            }
        }

        public MultiToolWindowChrome()
        {
            InitializeComponent();
            Loaded += MultiToolWindowChrome_Loaded;
            LayoutUpdated += MultiToolWindowChrome_LayoutUpdated;
            DataContext = this;
        }

        private void MultiToolWindowChrome_LayoutUpdated(object sender, System.EventArgs e)
        {
            SetBorderColor();
        }

        private void MultiToolWindowChrome_Loaded(object sender, RoutedEventArgs e)
        {
            SetBorderColor();
        }

        private void SetBorderColor()
        {
            DependencyObject dependencyObject = Parent;

            if (dependencyObject != null && dependencyObject is FrameworkElement parent)
            {
                while (parent != null && parent.Parent != null)
                {
                    parent = parent.Parent as FrameworkElement;
                }

                if (parent is Window parentWindow && parentWindow.BorderBrush != null)
                {
                    Brush parentBrush = parentWindow.BorderBrush;
                    ControlBorder.Background = parentBrush;

                    parentWindow.Activated += ParentWindow_Activated;
                    parentWindow.Deactivated += ParentWindow_Deactivated;
                }
                else
                {
                    ResourceDictionary resources = Application.Current.Resources;

                    object color = resources["DarkBlack"];
                    if (color is SolidColorBrush brush)
                    {
                        ControlBorder.Background = brush;
                    }
                    else
                    {
                        ControlBorder.Background = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }

        private void ParentWindow_Deactivated(object sender, System.EventArgs e)
        {
            Opacity = 0.5;
        }

        private void ParentWindow_Activated(object sender, System.EventArgs e)
        {
            Opacity = 1;
        }
    }
}
