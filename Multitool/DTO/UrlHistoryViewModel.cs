using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MultiTool.DTO
{
    public class UrlHistoryViewModel
    {
        public DateTime Date { get; set; }
        public string Link { get; set; }
        public Brush TextColor { get; set; }
    }
}
