using Multitool.JulieV2.Commands;

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Multitool.JulieV2
{
    public interface ILogParser
    {
        string LocateLogFile();
        StringBuilder ParseLogs(string directory);
    }
}
