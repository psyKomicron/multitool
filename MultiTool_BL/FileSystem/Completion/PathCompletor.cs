using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Multitool.FileSystem.Completion
{
    public class PathCompletor : IPathCompletor
    {
        private DirectoryInfo previousDir;

        public void Complete(string input, IList<string> list)
        {
            string fileName = GetFileName(input, out int i);
            string directory = GetDirName(input, i);
            string[] entries = GetEntries(directory);
            if (entries != null)
            {
                PutEntries(fileName, list, entries);
            }
        }

        private void PutEntries(string fileName, IList<string> list, string[] joins)
        {
            if (string.IsNullOrEmpty(fileName) || list.Count == 0) // no file name, list all directory
            {
                for (int j = 0; j < joins.Length; j++)
                {
                    list.Add(joins[j]);
                }
            }
            else // file name, search file name matches in the directory
            {
                string path;
                for (int j = 0; j < joins.Length; j++)
                {
                    path = GetFileName(joins[j]);
                    if (!path.StartsWith(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (list.Contains(joins[j]))
                        {
                            list.Remove(joins[j]);
                        }
                    }
                    else
                    {
                        if (!list.Contains(joins[j]))
                        {
                            list.Add(joins[j]);
                        }
                    }
                }
            }
        }

        private string GetFileName(string input, out int i)
        {
            if (input.Length > 1)
            {
                string fileName = string.Empty;
                for (i = input.Length - 1; i > 0; i--)
                {
                    if (input[i] == Path.DirectorySeparatorChar || input[i] == Path.AltDirectorySeparatorChar)
                    {
                        i++;
                        break;
                    }
                    fileName += input[i];
                }
                return Reverse(fileName);
            }
            else if (input.Length == 1)
            {
                i = 1;
                return input;
            }
            else
            {
                i = 0;
                return input;
            }
        }

        private string GetFileName(string input)
        {
            string fileName = string.Empty;
            for (int i = input.Length - 1; i > 0; i--)
            {
                if (input[i] == Path.DirectorySeparatorChar || input[i] == Path.AltDirectorySeparatorChar)
                {
                    break;
                }
                fileName += input[i];
            }
            return Reverse(fileName);
        }

        private string GetDirName(string input, int i)
        {
            string directory = null;
            if (input.Length < 4)
            {
                switch (input.Length)
                {
                    case 1:
                        directory = input + ':' + Path.DirectorySeparatorChar;
                        break;
                    case 2:
                        directory = input + Path.DirectorySeparatorChar;
                        break;
                    case 3:
                        directory = input;
                        break;
                }
            }
            else
            {
                directory = input.Substring(0, i);
            }
            return directory;
        }

        private string[] GetEntries(string path)
        {
            if (Directory.Exists(path))
            {
                previousDir = new DirectoryInfo(path);

                string[] directories = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);
                string[] joins = new string[directories.Length + files.Length];

                int j = 0;
                for (; j < files.Length; j++)
                {
                    joins[j] = files[j];
                }
                for (int i = 0; i < directories.Length; i++, j++)
                {
                    joins[j] = directories[i];
                }
                return joins;
            }
            else
            {
                return null;
            }
        }

        private string Reverse(string input)
        {
            string reverse = string.Empty;
            for (int i = input.Length - 1; i >= 0; i--)
            {
                reverse += input[i];
            }
            return reverse;
        }
    }
}
