
using System;
using System.IO;
using System.Text;

namespace Multitool.JulieV2.Logs
{
    public class LogParser : ILogParser
    {
        public string LocateLogFile()
        {
            throw new NotImplementedException();
        }

        public StringBuilder ParseLogs(string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException();
            }
            else
            {
                string filePath = string.Empty;
                StringBuilder builder = new StringBuilder(100);
                string[] files = Directory.GetFiles(directory);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".log"))
                    {
                        using (FileStream fs = new FileStream(filePath, FileMode.Open))
                        {
                            if (fs.CanRead)
                            {
                                UTF8Encoding encoding = new UTF8Encoding(false, true);
                                byte[] bytes = new byte[100];
                                int offset = 0;
                                while ((offset = fs.Read(bytes, 0, 50)) != 0)
                                {
                                    for (int n = 0; n < bytes.Length; n++)
                                    {
                                        try
                                        {
                                            builder.Append(encoding.GetString(bytes));
                                        }
                                        catch (ArgumentException e)
                                        {
                                            Console.Error.WriteLine(e.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return builder;
            }
        }
    }
}
