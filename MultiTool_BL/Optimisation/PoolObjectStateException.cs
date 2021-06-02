using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.Optimisation
{
    public class PoolObjectStateException : Exception
    {
        public PoolObjectStateException() : base("Object is not in a valid state") { }
        public PoolObjectStateException(string message) : base(message) { }
    }
}
