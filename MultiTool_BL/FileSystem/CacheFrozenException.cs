using System;

namespace MultiToolBusinessLayer.FileSystem
{
    internal class CacheFrozenException : Exception
    {
        public CacheFrozenException() : base("Cache is frozen") { }
    }
}
