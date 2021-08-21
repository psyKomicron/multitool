using System;

namespace Multitool.Parsers.Errors
{
    public class JsonFormatException : FormatException
    {
        public JsonFormatException(string message) : base(message)
        {

        }
    }
}
