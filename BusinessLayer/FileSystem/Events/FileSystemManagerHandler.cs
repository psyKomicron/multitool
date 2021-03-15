using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem.Events
{
    public delegate Task FileSystemManagerHandler(object sender, string message);
}
