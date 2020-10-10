using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer
{
    public class PowerController
    {
        public void Shutdown()
        {
            ProcessStartInfo process = new ProcessStartInfo("shutdown", "/s /t 0");
            process.CreateNoWindow = true;
            process.UseShellExecute = false;
            Process.Start(process);
        }

        public void Restart()
        {

        }

        public void Lock()
        {

        }

        public void Sleep()
        {

        }
    }
}
