using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.JulieV2.Logs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.JulieV2.Logs.Tests
{
    [TestClass()]
    public class LogParserTests
    {
        LogParser parser = new LogParser();

        [TestMethod()]
        public void LocateLogFileTest()
        {

        }

        [TestMethod()]
        public void ParseLogsTest()
        {
            StringBuilder logs = parser.ParseLogs(@"C:\User\julie\Documents\Github\JulieV2\files\logs\logfile.log");
        }
    }
}