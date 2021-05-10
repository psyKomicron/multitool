using Multitool.ProcessOptions.Enums;
using Multitool.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.Controllers
{
    public class PowerController : Controller<PowerOptions>
    {
        public override void Execute()
        {
            ProcessStartInfo process = new ProcessStartInfo("shutdown", GetOptions())
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process.Start(process);
        }
    }
}
