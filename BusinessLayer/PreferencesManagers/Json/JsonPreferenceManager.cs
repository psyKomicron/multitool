using BusinessLayer.Parsers;
using BusinessLayer.Parsers.Errors;
using BusinessLayer.Reflection.ObjectFlatteners;
using BusinessLayer.Reflection.PropertyLoaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BusinessLayer.PreferencesManagers.Json
{
    public class JsonPreferenceManager : IPreferenceManager
    {
        private List<WindowPreferenceManager> childs = new List<WindowPreferenceManager>(5);
        private readonly PropertyLoader propertyLoader = new PropertyLoader();

        public string Path { get; set; }

        public PreferenceManagerType Type => PreferenceManagerType.JSON;

        public void SerializePreferenceManager()
        {
            string path = Path; // to keep the same path even if the property is changed
            JsonPreferenceManager data = ParsePreferences(path);

            if (data == null)
            {
                WritePreferences(CreateData(), path);
            }
            else
            {
                bool equivalent = false;
                // compare and write to optimize performance
                for (int i = 0; i < data.childs.Count && i < childs.Count && equivalent; i++)
                {
                    equivalent = data.childs[i].IsEquivalentTo(childs[i]);
                }
                if (!equivalent)
                {
                    WritePreferences(CreateData(), path);
                }
                else
                {
                    Console.WriteLine("\tCurrent preferences are the same as the saved ones, no writing required");
                }
            }
        }

        public void DeserializePreferenceManager()
        {
            JsonPreferenceManager manager = ParsePreferences(Path);
            if (manager != null)
            {
                childs = manager.childs;
            }
        }

        public void AddWindowManager<DataType>(DataType data, string name) where DataType : class
        {
            WindowPreferenceManager manager = new WindowPreferenceManager()
            {
                Properties = new BasicObjectFlattener().Flatten(data),
                ItemName = name
            };

            if (childs.Contains(manager)) // WPM implements IQuatable
            {
                WindowPreferenceManager currentManager = null; 
                foreach (var child in childs)
                {
                    if (child.ItemName.Equals(manager.ItemName))
                    {
                        currentManager = child;
                    }
                }
                if (currentManager != null && !currentManager.IsEquivalentTo(manager))
                {
                    childs.Remove(currentManager);
                    childs.Add(manager);
                }
            }
            else
            {
                childs.Add(manager);
            }
        }

        public DataType GetWindowManager<DataType>(string name) where DataType : class, new()
        {
            foreach (WindowPreferenceManager manager in childs)
            {
                if (manager.ItemName.Equals(name))
                {
                    return propertyLoader.LoadFromStringDictionary<DataType>(manager.Properties);
                }
            }
            return null;
        }

        internal JsonPreferenceManager ParsePreferences(string path)
        {
            try
            {
                string rawData = ReadPreferenceFile(path);
                string data = string.Empty;
                bool inString = false;

                for (int i = 0; i < rawData.Length; i++)
                {
                    if (rawData[i] == '"')
                    {
                        inString = !inString;
                    }

                    if (IsControlChar(rawData[i]))
                    {
                        if (inString)
                        {
                            data += rawData[i];
                        }
                    }
                    else
                    {
                        data += rawData[i];
                    }
                }

                return JsonParser.ParsePreferenceManager(data);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine("Not able to find \"" + path + "\"");
            }
            catch (JsonFormatException jfe)
            {
                Console.Error.WriteLine(jfe.Message);
            }
            return null;
        }

        internal void AddPreferenceManagers(List<WindowPreferenceManager> managers)
        {
            childs = managers;
        }

        private string CreateData()
        {
            StringBuilder data = new StringBuilder();
            data.Append("{\"preferences\":[");
            for (int i = 0; i < childs.Count; i++)
            {
                data.Append(Flatten(childs[i])).Append("}");
                if (i != childs.Count - 1)
                {
                    data.Append(",");
                }
            }
            data.Append("]}");
            return data.ToString();
        }

        private StringBuilder Flatten(WindowPreferenceManager manager)
        {
            StringBuilder json = new StringBuilder("{\"" + manager.ItemName + "\":{");
            KeyValuePair<string, string> last = manager.Properties.Last();
            foreach (KeyValuePair<string, string> pair in manager.Properties)
            {
                json.Append(Jsonify(pair));
                if (!last.Key.Equals(pair.Key))
                {
                    json.Append(",");
                }
            }
            return json.Append("}");
        }

        private string ReadPreferenceFile(string path)
        {
            byte[] bytes;
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                bytes = new byte[stream.Length];
                int bytesToRead = (int)stream.Length;
                int bytesRead = 0;
                int n = 0;
                while ((n = stream.Read(bytes, bytesRead, bytesToRead)) != 0)
                {
                    bytesRead += n;
                    bytesToRead -= n;
                }
            }
            return Encoding.UTF8.GetString(bytes);
        }

        private void WritePreferences(string data, string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(data.ToCharArray());
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }

        private bool IsControlChar(char c) => c == '\r' || c == '\n' || c == '\t' || c == ' ';

        private string Jsonify(KeyValuePair<string, string> pair) => "\"" + pair.Key + "\":\"" + pair.Value + "\"";

    }
}
