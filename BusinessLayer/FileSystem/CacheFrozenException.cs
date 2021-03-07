using System;

namespace BusinessLayer.FileSystem
{
    internal class CacheFrozenException : Exception
    {
        public CacheFrozenException() : base("Cache is frozen") { }
    }
}
