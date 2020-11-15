using System.Collections.Generic;

namespace BusinessLayer.PreferencesManager
{
    public interface IWindowPreferenceManager
    {
        string ItemName { get; set; }
        Dictionary<string, string> Values { get; set; }

        bool IsEquivalentTo(Dictionary<string, string> data);
        bool IsEquivalentTo(IWindowPreferenceManager manager);
        bool Equals(JsonWindowPreferenceManager other);
    }
}