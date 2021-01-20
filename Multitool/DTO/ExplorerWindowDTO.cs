﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTool.DTO
{
    public class ExplorerWindowDTO : DefaultWindowDTO
    {
        public string LastUsedPath { get; set; }

        public ExplorerWindowDTO()
        {
            LastUsedPath = string.Empty;
        }
    }
}
