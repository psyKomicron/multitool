using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.Parsers.Errors
{
    public class JsonFormatException : FormatException
    {
        public JsonFormatException(string message) : base(message)
        {
            
        }
    }
}
