using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PreferencesManager
{
    public class XmlWindowPreferenceManager : IWindowPreferenceManager
    {
        public string ItemName { get; set; }
        public Dictionary<string, string> Values { get; set; }

        public bool Equals(JsonWindowPreferenceManager other)
        {
            throw new NotImplementedException();
        }

        public bool IsEquivalentTo(Dictionary<string, string> data)
        {
            throw new NotImplementedException();
        }

        public bool IsEquivalentTo(IWindowPreferenceManager manager)
        {
            throw new NotImplementedException();
        }
    }
}
