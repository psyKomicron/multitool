using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text;

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