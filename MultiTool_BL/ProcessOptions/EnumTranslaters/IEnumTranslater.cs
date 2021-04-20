using System;
using System.Collections.Generic;

namespace MultiToolBusinessLayer.ProcessOptions.EnumTranslaters
{
    public interface IEnumTranslater<EnumType> where EnumType : Enum
    {
        string Translate(List<EnumType> enums);
    }
}
