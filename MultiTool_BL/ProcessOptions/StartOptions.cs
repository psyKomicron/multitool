﻿using Multitool.ProcessOptions.EnumTranslaters;
using System;
using System.Collections.Generic;

namespace Multitool.ProcessOptions
{
    public class StartOptions<EnumType> where EnumType : Enum
    {
        public List<EnumType> Options { get; set; }
        public IEnumTranslater<EnumType> Translater { get; set; }
    }
}
