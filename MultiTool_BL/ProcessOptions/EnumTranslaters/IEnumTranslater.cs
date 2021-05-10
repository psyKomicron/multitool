using System;
using System.Collections.Generic;

namespace Multitool.ProcessOptions.EnumTranslaters
{
    public interface IEnumTranslater<EnumType> where EnumType : Enum
    {
        string Translate(List<EnumType> enums);
    }
}
