using MultiToolBusinessLayer.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;

namespace MultiToolBusinessLayer.ProcessOptions
{
    public class StartOptions<EnumType> where EnumType : Enum
    {
        public List<EnumType> Options { get; set; }
        public IEnumTranslater<EnumType> Translater { get; set; }
    }
}
