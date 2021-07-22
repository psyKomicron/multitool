using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        public TextEditor()
        {
            InitializeComponent();
        }

        public Point DrawingPosition { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // visual objects
            FormattedText ft = new FormattedText("Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                16,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            drawingContext.DrawText(ft, new Point(5, 5));
            //base.OnRender(drawingContext);
        }
    }
}
