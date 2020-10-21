using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PreferencesManager
{
    public class PreferenceManager
    {
        private List<WindowPreferenceManager> childs = new List<WindowPreferenceManager>(5);

        /// <summary>
        /// Serialize the childs.
        /// </summary>
        /// <param name="path"></param>
        public void SavePreferences(string path = "C:\\Users\\julie\\Documents\\MultiTool\\preferences\\userpreferences.json")
        {
            var data = ParsePreferences(path);
            if (data == null)
            {
                WritePreferences(CreateData(), path);
            }
            else
            {
                // compare and write to optimize performance

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
            if (childs.Contains(manager))
            {
                var currentManager = childs.((item) => item.ItemName == manager.ItemName);
                //if ()
            }
            else
            {
                childs.Add(manager);
            }
            return manager;
        }

        private List<WindowPreferenceManager> ParsePreferences(string path)
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
                string data = Encoding.UTF8.GetString(bytes);

                //ParseJson(data);
            }
            catch (FileNotFoundException) 
            {
                Console.Error.WriteLine("Not able to find \"" + path + "\"");
            }
            return null;
        }


        private Dictionary<string, string> ParseJson(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
               // if (s[i] == '{')
            }
            return null;
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
    }
}
