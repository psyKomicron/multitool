using Multitool.ProcessOptions.Enums;

using System.Diagnostics;

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
