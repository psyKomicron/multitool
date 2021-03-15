using System;

namespace BusinessLayer.FileSystem.Events
{
    internal class TTLReachedEventArgs : EventArgs
    {
        public TTLReachedEventArgs(string path, FileSystemCache cache, double ttl, bool ttlUpdated = false) : base()
        {
            Path = path;
            Cache = cache;
            TTLUpdated = ttlUpdated;
            TTL = ttl;
        }

        public FileSystemCache Cache { get; private set; }

        public string Path { get; private set; }

        public bool TTLUpdated { get; private set; }

        public double TTL { get; private set; }
    }
}
