namespace MultiToolBusinessLayer.JulieV2
{
    public class User
    {
        public uint Discriminator { get; internal set; }
        public string Name { get; internal set; }
        public string Tag { get; internal set; }
    }
}
