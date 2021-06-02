using System;

namespace Multitool.JulieV2
{
    public class User : IEquatable<User>
    {
        public uint Discriminator { get; internal set; }
        public string Tag { get; internal set; }
        public string Name { get; internal set; }

        public bool Equals(User other)
        {
            return Discriminator.Equals(other.Discriminator) && Tag.Equals(other.Tag) && Name.Equals(other.Name);
        }
    }
}
