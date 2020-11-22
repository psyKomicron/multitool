using BusinessLayer.Parsers;
using BusinessLayer.Parsers.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusinessLayer.PreferencesManagers.Json
{
    public class JsonPreferenceManager : IPreferenceManager<JsonWindowPreferenceManager>
    {
        private List<JsonWindowPreferenceManager> childs = new List<JsonWindowPreferenceManager>(5);    

        public string Path { get; set; }

        /// <summary>
        /// Serialize the childs to a JSON file.
        /// </summary>
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

        /// <summary>
        /// Add a <see cref="JsonWindowPreferenceManager"/> to the list.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        public JsonWindowPreferenceManager AddPreferenceManager(JsonWindowPreferenceManager manager)
        {
            if (childs.Contains(manager)) // WPM implements IQuatable
            {
                JsonWindowPreferenceManager currentManager = null; 
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
            return manager;
        }

        public JsonWindowPreferenceManager GetWindowManager(string name)
        {
            foreach (JsonWindowPreferenceManager manager in childs)
            {
                if (manager.ItemName.Equals(name))
                {
                    return manager;
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

        internal void AddPreferenceManagers(List<JsonWindowPreferenceManager> managers)
        {
            childs = managers;
        }

        private string CreateData()
        {
            StringBuilder data = new StringBuilder();
            data.Append("{\"preferences\":[");
            for (int i = 0; i < childs.Count; i++)
            {
                data.Append(childs[i].Flatten()).Append("}");
                if (i != childs.Count - 1)
                {
                    data.Append(",");
                }
            }
            data.Append("]}");
            return data.ToString();
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

    }
}
