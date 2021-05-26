using System;

namespace Multitool.NTInterop
{
    public class PowerPlan
    {
        private Guid guid;
        private string name;

        internal PowerPlan(Guid guid, string name)
        {
            this.guid = guid;
            this.name = name;
        }

        public Guid Guid => guid;
        public string Name => name;
    }
}
