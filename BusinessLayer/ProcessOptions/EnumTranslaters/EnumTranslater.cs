using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.ProcessOptions.EnumTranslaters
{
    public interface EnumTranslater<EnumType> where EnumType : Enum
    {
        string Translate(List<EnumType> enums);
    }
}
