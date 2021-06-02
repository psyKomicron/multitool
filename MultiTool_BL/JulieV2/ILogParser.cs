using System.Collections.Generic;
using System.IO;

namespace Multitool.JulieV2
{
    public interface ILogParser
    {
        string LocateLogFile();
        IList<BotCommand> ParseLogs(string directory);
    }
}
