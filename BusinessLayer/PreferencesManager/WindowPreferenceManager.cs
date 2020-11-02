using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BusinessLayer.PreferencesManager
{
    public class WindowPreferenceManager : IEquatable<WindowPreferenceManager>
    {
        public Dictionary<string, string> Values { get; set; }
        public string ItemName { get; set; }

        public bool Equals(WindowPreferenceManager other)
        {
            return other.ItemName.Equals(ItemName);
        }

        internal StringBuilder Flatten()
        {
            StringBuilder json = new StringBuilder("{\"" + ItemName + "\":{");
            KeyValuePair<string, string> last = Values.Last();
            foreach (KeyValuePair<string, string> pair in Values)
            {
                json.Append(this.Json(pair));
                if (!last.Key.Equals(pair.Key))
                {
                    json.Append(",");
                }
            }
            return json.Append("}");
        }

        /// <summary>
        /// Compare if a <see cref="WindowPreferenceManager.Values"/> and a <see cref="Dictionary{TKey, TValue}"/> have the same stored values.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        internal bool IsEquivalentTo(Dictionary<string, string> data)
        {
            if (data.Count <= Values.Count)
            {
                foreach (KeyValuePair<string, string> pair in data)
                {
                    if (Values.ContainsKey(pair.Key))
                    {
                        string currentValue = Values[pair.Key];
                        if (currentValue == null)
                        {
                            return false;
                        }
                        else if (!currentValue.Equals(pair.Value))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        internal bool IsEquivalentTo(WindowPreferenceManager manager)
        {
            return IsEquivalentTo(manager.Values);
        }

        private string Json(KeyValuePair<string, string> pair)
        {
            return "\"" + pair.Key + "\":\"" + pair.Value + "\"";
        }
    }
}
