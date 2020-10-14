using BusinessLayer.ProcessOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Controllers
{
    public abstract class Controller<OptionsType> where OptionsType : Enum
    {
        public StartOptions<OptionsType> StartOptions { get; set; }

        public abstract void Execute();

        public void ClearOptions()
        {
            if (StartOptions.Options != null)
            {
                StartOptions.Options.Clear();
            }
        }

        protected string GetOptions()
        {
            return StartOptions.Translater.Translate(StartOptions.Options);
        }
    }
}
