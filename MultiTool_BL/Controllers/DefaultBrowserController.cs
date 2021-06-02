using Multitool.ProcessOptions.Enums;
using System;
using System.Diagnostics;

namespace Multitool.Controllers
{
    public class DefaultBrowserController : Controller<DefaultOptions>
    {
        public Uri Uri { get; set; }

        public override void Execute()
        {
            Process.Start(Uri.AbsoluteUri);
        }
    }
}
