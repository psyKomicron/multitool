namespace MultiTool.DTO
{
    public class PowerWindowDTO : DefaultWindowDTO
    {
        public bool ForceShutdown { get; set; }

        public PowerWindowDTO()
        {
            Height = 430;
            Width = 680;
            ForceShutdown = true;
        }
    }
}
