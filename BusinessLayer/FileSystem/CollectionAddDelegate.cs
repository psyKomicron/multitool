﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.FileSystem
{
    public delegate void CollectionAddDelegate<ItemType>(IList<ItemType> items, PathItem item) where ItemType : IPathItem;
}
