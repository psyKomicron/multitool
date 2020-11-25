using System;
using System.Collections.Generic;

namespace BusinessLayer.ProcessOptions.EnumTranslaters
{
    public interface IEnumTranslater<EnumType> where EnumType : Enum
    {
        string Translate(List<EnumType> enums);
    }
}
