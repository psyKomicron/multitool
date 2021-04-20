using System.IO;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MultiToolBusinessLayer.Parsers
{
    public class UriCleaner
    {
        List<char> invalidChars;

        public UriCleaner()
        {
            GetInvalidRegex(GetInvalidChars());
        }

        public string RemoveControlChars(in string s)
        {
            StringBuilder stringBuilder = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; i++)
            {
                for (int j = 0; j < invalidChars.Count; j++)
                {
                    if (s[i] != invalidChars[j])
                    {
                        stringBuilder.Append(s[i]);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Removes control chars generated when pressing [RETURN] key.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string RemoveChariotReturns(in string s)
        {
            return Regex.Replace(s, "(\n|\r)", string.Empty, RegexOptions.Compiled & RegexOptions.IgnoreCase & RegexOptions.Multiline);
        }

        public bool HasForbiddenChar(string s)
        {
            for (int i = 0; i < invalidChars.Count; i++)
            {
                if (i < s.Length)
                {
                    if (s[i] == invalidChars[i])
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private bool IsRegexReserved(char c)
        {
            return (c == '|' ||
                    c == '.' ||
                    c == '(' ||
                    c == ')' ||
                    c == '[' ||
                    c == ']' ||
                    c == '{' ||
                    c == '}' ||
                    c == '*' ||
                    c == '+' ||
                    c == '^' ||
                    c == '$' ||
                    c == '/' ||
                    c == '-' ||
                    c == '\\');
        }

        private char[] GetInvalidChars()
        {
            char[] invalidPathChars = Path.GetInvalidPathChars();
            char[] invalidFileChars = Path.GetInvalidFileNameChars();

            char[] invalids = new char[invalidPathChars.Length + invalidFileChars.Length];

            for (int i = 0; i < invalidPathChars.Length; i++)
            {
                invalids[i] = invalidPathChars[i];
            }

            int actualSize = invalidPathChars.Length;

            for (int i = 0; i < invalidFileChars.Length; i++)
            {
                char fileChar = invalidFileChars[i];

                bool contains = false;
                for (int j = 0; j < invalidPathChars.Length; j++)
                {
                    if (fileChar == invalidPathChars[j])
                    {
                        contains = true;
                        break;
                    }
                }

                if (!contains)
                {
                    if (actualSize < invalids.Length)
                    {
                        // adding
                        invalids[actualSize] = fileChar;
                        actualSize++;
                    }
                }
            }

            return invalids;
        }

        private void GetInvalidRegex(char[] chars)
        {
            invalidChars = new List<char>(chars.Length);

            for (int i = 0; i < chars.Length; i++)
            {
                char currentChar = chars[i];
                if (currentChar != 0)
                {
                    if (IsRegexReserved(currentChar))
                    {
                        invalidChars.Add('\\');
                        invalidChars.Add(currentChar);
                    }
                    else
                    {
                        invalidChars.Add(currentChar);
                    }

                    if (!(i == chars.Length - 1))
                    {
                        invalidChars.Add('|');
                    }
                }
            }
        }
    }
}
