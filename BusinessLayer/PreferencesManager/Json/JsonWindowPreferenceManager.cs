using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.PreferencesManagers.Json
{
    public class JsonWindowPreferenceManager : WindowPreferenceManager, IEquatable<JsonWindowPreferenceManager>
    {
        public bool Equals(JsonWindowPreferenceManager other)
        {
            return base.Equals(other);
        }

        public StringBuilder Flatten()
        {
            StringBuilder json = new StringBuilder("{\"" + ItemName + "\":{");
            KeyValuePair<string, string> last = Properties.Last();
            foreach (KeyValuePair<string, string> pair in Properties)
            {
                json.Append(this.Json(pair));
                if (!last.Key.Equals(pair.Key))
                {
                    json.Append(",");
                }
            }
            return json.Append("}");
        }

        private string Json(KeyValuePair<string, string> pair)
        {
            return "\"" + pair.Key + "\":\"" + pair.Value + "\"";
        }
    }
}
