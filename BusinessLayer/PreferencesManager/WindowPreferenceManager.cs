using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.PreferencesManagers
{
    public abstract class WindowPreferenceManager : IWindowPreferenceManager, IEquatable<WindowPreferenceManager>
    {
        public string ItemName { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public virtual bool IsEquivalentTo(Dictionary<string, string> data)
        {
            if (data.Count == Properties.Count)
            {
                foreach (KeyValuePair<string, string> pair in data)
                {
                    string currentValue = Properties.ContainsKey(pair.Key) ? Properties[pair.Key] : null;
                    if (currentValue == null)
                    {
                        return false;
                    }
                    else if (!currentValue.Equals(pair.Value))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsEquivalentTo(IWindowPreferenceManager manager)
        {
            return IsEquivalentTo(manager.Properties);
        }

        public bool Equals(WindowPreferenceManager other)
        {
            return other.ItemName.Equals(other.ItemName);
        }
    }
}
