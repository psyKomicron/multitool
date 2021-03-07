using System;

namespace BusinessLayer.FileSystem.Events
{
    internal class TTLReachedEventArgs : EventArgs
    {
        public TTLReachedEventArgs(string path, FileSystemCache cache, bool fromTTL) : base()
        {
            Path = path;
            Cache = cache;
            FromTTL = fromTTL;
        }

        public string Path { get; private set; }

        public FileSystemCache Cache { get; private set; }

        public bool FromTTL { get; private set; }
    }
}
