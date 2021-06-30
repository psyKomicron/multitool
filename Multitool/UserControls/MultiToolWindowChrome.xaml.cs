using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for WindowChrome.xaml
    /// </summary>
    public partial class MultitoolWindowChrome : UserControl
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(MultitoolWindowChrome));

        private uint closeListeners = 0;
        private uint minimizeListeners = 0;
        private uint maximizedListeners = 0;
        private Window parentWindow;

        public MultitoolWindowChrome()
        {
            InitializeComponent();
            Loaded += MultiToolWindowChrome_Loaded;
            LayoutUpdated += MultiToolWindowChrome_LayoutUpdated;
            DataContext = this;
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        #region events

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

        public event RoutedEventHandler MaximizeClick
        {
            add
            {
                WindowMaximizeButton.Click += value;
                if (!WindowMaximizeButton.IsEnabled)
                {
                    WindowMaximizeButton.IsEnabled = true;
                }
                maximizedListeners++;
            }
            remove
            {
                WindowMaximizeButton.Click -= value;

                if (maximizedListeners > 0)
                {
                    maximizedListeners--;
                }

                if (maximizedListeners == 0)
                {
                    WindowMaximizeButton.IsEnabled = false;
                }
            }
        }

        #endregion

        #region private

        private void MultiToolWindowChrome_LayoutUpdated(object sender, System.EventArgs e)
        {
            SetBorderColor();
        }

        private void MultiToolWindowChrome_Loaded(object sender, RoutedEventArgs e)
        {
            if (parentWindow == null)
            {
                parentWindow = GetParentWindow();
            }
            SetBorderColor();
        }

        private Window GetParentWindow()
        {
            DependencyObject dependencyObject = Parent;
            if (dependencyObject != null && dependencyObject is FrameworkElement parent)
            {
                while (parent != null && parent.Parent != null)
                {
                    parent = parent.Parent as FrameworkElement;
                }
                return parent as Window;
            }
            return null;
        }

        private void SetBorderColor()
        {
            if (parentWindow != null)
            {
                if (Background == null)
                {
                    if (parentWindow.BorderBrush != null)
                    {
                        Brush parentBrush = parentWindow.BorderBrush;
                        ControlBorder.BorderBrush = parentBrush;
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

                parentWindow.Activated += ParentWindow_Activated;
                parentWindow.Deactivated += ParentWindow_Deactivated;
            }
        }

        #endregion

        #region events handlers
        private void ParentWindow_Deactivated(object sender, System.EventArgs e)
        {
            Opacity = 0.5;
        }

        private void ParentWindow_Activated(object sender, System.EventArgs e)
        {
            Opacity = 1;
        }
        #endregion
    }
}
