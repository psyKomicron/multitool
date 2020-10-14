using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ProcessOptions.Enums
{
    public enum PowerOptions
    {
        LogOff,
        Shutdown,
        Restart,
        SilentShutdown,
        Hibernate,
        Hybrid,
        FirmwareUINextBoot,
        NoDelay,
        Force
    }
}
