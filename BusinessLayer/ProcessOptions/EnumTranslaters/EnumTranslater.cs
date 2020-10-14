using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ProcessOptions.EnumTranslaters
{
    public abstract class EnumTranslater<EnumType> where EnumType : Enum
    {
        public abstract string Translate(List<EnumType> enums);
    }
}
