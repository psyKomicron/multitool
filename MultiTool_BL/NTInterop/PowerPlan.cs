using System;

namespace Multitool.NTInterop
{
    public class PowerPlan
    {
        private Guid _guid;
        private string _name;
        private bool _active;

        internal PowerPlan(Guid guid, string name, bool active)
        {
            _guid = guid;
            _name = name;
            _active = active;
        }

        public Guid Guid => _guid;
        public string Name => _name;
        public bool Active => _active;

        public override string ToString()
        {
            return "{Guid: " + Guid + ", Name " + Name + "}";
        }
    }
}
