using BusinessLayer.Parsers.Errors;
using BusinessLayer.PreferencesManager;
using System;
using System.Collections.Generic;

namespace BusinessLayer.Parsers
{
    internal static class JsonParser
    {
        public static JsonPreferenceManager ParsePreferenceManager(string s)
        {
            if (!string.IsNullOrEmpty(s) && s[0] == '{')
            {
                JsonPreferenceManager preferenceManager = new JsonPreferenceManager();
                string rootName = GetJsonName(s.Substring(1));

                if (s.Length > rootName.Length + 4 && s[rootName.Length + 4] == '[') // array
                {
                    int startIndex = rootName.Length + 4;
                    int endIndex = startIndex;
                    bool inString = false;
                    while (s[endIndex] != ']' || inString)
                    {
                        if (s[endIndex] == '"')
                        {
                            inString = !inString;
                        }

                        endIndex++;
                    }
                    string array = s.Substring(startIndex, 1 + endIndex - startIndex);

                    List<JsonWindowPreferenceManager> childs = ParseWindowPreferenceManager(array.Substring(1, array.Length - 2));
                    preferenceManager.AddPreferenceManagers(childs);
                    
                }
                return preferenceManager;
            }
            else
            {
                throw new JsonFormatException("Given JSON (as string) could not be transformed into a preference manager.");
            }
        }

        public static Dictionary<string, string> Parse(string json, out int charRead)
        {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            charRead = 0;

            if (json[0] == '{')
            {
                charRead++;
                while (charRead < json.Length && json[charRead] != '}')
                {
                    if (json[charRead] == '"')
                    {
                        charRead++;
                        int lengthName = 0;
                        while (lengthName + charRead < json.Length && json[lengthName + charRead] != '"')
                        {
                            lengthName++;
                        }
                        string propName = json.Substring(charRead, lengthName);

                        charRead += lengthName + 3;
                        // value
                        lengthName = 0;
                        while (lengthName + charRead < json.Length && json[lengthName + charRead] != '"')
                        {
                            lengthName++;
                        }
                        string value = json.Substring(charRead, lengthName);

                        charRead += lengthName + 2;

                        properties.Add(propName, value);
                    }
                }
            } 
            return properties;
        }

        private static List<JsonWindowPreferenceManager> ParseWindowPreferenceManager(string s)
        {
            List<JsonWindowPreferenceManager> managers = new List<JsonWindowPreferenceManager>(s.Length / 100);

            int it = 1;
            int charRead = 0;

            while (it < s.Length)
            {
                if (s[it] == '"') // property
                {
                    string name = string.Empty;
                    int i = it + 1;
                    while (i < s.Length && s[i] != '"')
                    {
                        name += s[i];
                        i++;
                    }
                    charRead += i;

                    JsonWindowPreferenceManager manager = new JsonWindowPreferenceManager() { ItemName = name };
                    managers.Add(manager);
                    
                    manager.Values = Parse(s.Substring(charRead + 2),  out int charParsed);
                    charRead += charParsed;
                }
                // end
                it += charRead == 0 ? 1 : charRead; // to avoid overflow
                charRead = 0;
            }

            managers.TrimExcess();
            return managers;
        }

        private static string GetJsonName(string s)
        {
            string name = string.Empty;
            if (s[0] == '"')
            {
                int i = 1;
                while (s[i] != '"' && i < 255)
                {
                    name += s[i];
                    i++;
                }
            }
            else
            {
                throw new Exception("Cannot parse name from string");
            }
            return name;
        }
    }
}
