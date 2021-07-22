namespace MultitoolWPF.ViewModels
{
    public class MainWindowData : DefaultWindowData
    {
        public MainWindowData()
        {
            Height = 550;
            Width = 800;
        }

        public int LastSelectedIndex { get; set; }
        public string StartWindow { get; set; }
    }
}
