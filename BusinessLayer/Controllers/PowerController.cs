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
        /// <summary>
        /// Defaults the Translater to <see cref="PowerEnumTranslater"/>
        /// </summary>
        public PowerController()
        {
            StartOptions.Translater = new PowerEnumTranslater();
        }

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
