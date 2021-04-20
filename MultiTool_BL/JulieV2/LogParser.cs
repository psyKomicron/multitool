using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiToolBusinessLayer.JulieV2
{
    public class LogParser : ILogParser
    {
        public string LocateLogFile()
        {
            throw new NotImplementedException();
        }

        public IList<BotCommand> ParseLogs(string directory)
        {
            List<BotCommand> commands = new List<BotCommand>(100);

            commands.TrimExcess();
            return commands;
        }
    }
}
