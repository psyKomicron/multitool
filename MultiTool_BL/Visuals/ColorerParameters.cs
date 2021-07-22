using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.Visuals
{
    public class ColorerParameters
    {
        public List<string> Keywords { get; internal set; }
        public Color KeywordColor { get; internal set; }

        public List<string> StructureChars { get; internal set; }
        public Color StructureCharColor { get; internal set; }

        public Color DefaultCharColor { get; internal set; }
    }
}
