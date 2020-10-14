using BusinessLayer.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ProcessOptions
{
    public class StartOptions<EnumType> where EnumType : Enum
    {
        public List<EnumType> Options { get; set; }
        public EnumTranslater<EnumType> Translater { get; set; }
    }
}
