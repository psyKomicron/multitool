using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Reflection
{
    public interface IObjectFlattener
    {
        Dictionary<string, string> Flatten<T>(T o) where T : class;
    }
}
