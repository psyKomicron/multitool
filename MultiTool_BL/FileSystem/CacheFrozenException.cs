using System;

namespace Multitool.FileSystem
{
    internal class CacheFrozenException : Exception
    {
        public CacheFrozenException() : base("Cache is frozen") { }
    }
}
