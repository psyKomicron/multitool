namespace MultitoolWPF.ViewModels
{
    public class PowerWindowData : DefaultWindowData
    {
        public bool ForceShutdown { get; set; }

        public PowerWindowData()
        {
            Height = 430;
            Width = 680;
            ForceShutdown = true;
        }
    }
}
