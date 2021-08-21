
using System;

namespace Multitool.NTInterop.Power
{
    public class PowerPlan
    {
        private bool _active;

        internal PowerPlan(Guid guid, string name, bool active)
        {
            PowerOptions.ActiveChanged += PowerOptions_ActiveChanged;
            Guid = guid;
            Name = name;
            Active = active;
        }

        public Guid Guid { get; }
        public string Name { get; }
        public bool Active
        {
            get => _active;
            internal set
            {
                if (_active != value)
                {
                    _active = value;
                    StatusChanged?.Invoke(this, EventArgs.Empty);
#if DEBUG
                    if (value)
                    {
                        Console.WriteLine(Name + " switched to active");
                    }
#endif
                }
            }
        }

        public event EventHandler StatusChanged;

        public override string ToString()
        {
            return "{Guid: " + Guid + ", Name " + Name + "}";
        }

        private void PowerOptions_ActiveChanged(Guid newPowerPlan)
        {
            Active = newPowerPlan == Guid;
        }
    }
}
