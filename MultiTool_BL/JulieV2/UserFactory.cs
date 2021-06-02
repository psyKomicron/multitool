namespace Multitool.JulieV2
{
    public static class UserFactory
    {
        public static User Create(uint discriminator, string tag, string name)
        {
            return new User()
            {
                Discriminator = discriminator,
                Tag = tag,
                Name = name
            };
        }
    }
}
