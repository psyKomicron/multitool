using BusinessLayer.ProcessOptions.Enums;
using BusinessLayer.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Controllers
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
