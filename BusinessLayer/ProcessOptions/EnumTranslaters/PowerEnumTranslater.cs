using BusinessLayer.ProcessOptions.Enums;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.ProcessOptions.EnumTranslaters
{
    public class PowerEnumTranslater : IEnumTranslater<PowerOptions>
    {
        public string Translate(List<PowerOptions> enums)
        {
            StringBuilder options = new StringBuilder(10);
            enums.ForEach((value) =>
            {
                switch (value)
                {
                    case PowerOptions.LogOff:
                        options.Append("/l ");
                        break;
                    case PowerOptions.Shutdown:
                        options.Append("/s ");
                        break;
                    case PowerOptions.Restart:
                        options.Append("/r ");
                        break;
                    case PowerOptions.SilentShutdown:
                        options.Append("/p ");
                        break;
                    case PowerOptions.Hibernate:
                        options.Append("/h ");
                        break;
                    case PowerOptions.Hybrid:
                        options.Append("/hybrid ");
                        break;
                    case PowerOptions.FirmwareUINextBoot:
                        options.Append("/fw ");
                        break;
                    case PowerOptions.NoDelay:
                        options.Append("/t 0 ");
                        break;
                    case PowerOptions.Force:
                        options.Append("/f ");
                        break;
                }
            });
            return options.ToString();
        }
    }
}
