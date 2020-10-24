using BusinessLayer.Parsers;
using BusinessLayer.Parsers.Errors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PreferencesManager
{
    public class PreferenceManager
    {
        private List<WindowPreferenceManager> childs = new List<WindowPreferenceManager>(5);

        public string Path { get; private set; }

        public PreferenceManager()
        {
            Path = @"C:\Users\julie\Documents\MultiTool\preferences\userpreferences.json";
        }

        public PreferenceManager(string path)
        {
            Path = path;
        }

        /// <summary>
        /// Serialize the childs.
        /// </summary>
        /// <param name="path"></param>
        public void SavePreferences()
        {
            PreferenceManager data = ParsePreferences(Path);

            if (data == null)
            {
                WritePreferences(CreateData(), Path);
            }
            else
            {
                bool noWrite = true;
                // compare and write to optimize performance
                for (int i = 0; i < data.childs.Count && i < childs.Count && noWrite; i++)
                {
                    noWrite = !data.childs[i].IsEquivalentTo(childs[i]);
                }
                if (noWrite)
                {
                    WritePreferences(CreateData(), Path);
                }
            }
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

        public WindowPreferenceManager AddPreferenceManager(WindowPreferenceManager manager)
        {
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
            return manager;
        }

        internal void AddPreferenceManagers(List<WindowPreferenceManager> managers)
        {
            childs = managers;
        }

        private PreferenceManager ParsePreferences(string path)
        {
            try
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
                string rawData = Encoding.UTF8.GetString(bytes);
                string data = string.Empty;
                bool inString = false;

                for (int i = 0; i < rawData.Length; i++)
                {
                    if (rawData[i] == '"')
                    {
                        if (inString)
                        {
                            inString = false;
                        }
                        else
                        {
                            inString = true;
                        }
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
                Console.WriteLine(data);
                PreferenceManager manager = null;
                try
                {
                    manager = JsonParser.ParsePreferenceManager(data);
                }
                catch (JsonFormatException jfe)
                {
                    Console.Error.WriteLine(jfe.Message);
                }

                return manager;
            }
            catch (FileNotFoundException) 
            {
                Console.Error.WriteLine("Not able to find \"" + path + "\"");
            }
            return null;
        }

        private bool IsControlChar(char c) => c == '\r' || c == '\n' || c == '\t' || c == ' ';

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
    }
}
