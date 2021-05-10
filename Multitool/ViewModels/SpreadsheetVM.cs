using System;

namespace MultiTool.ViewModels
{
    public class SpreadsheetVM : IComparable<SpreadsheetVM>
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Ranking { get; set; }

        public int CompareTo(SpreadsheetVM other)
        {
            if (Ranking > other.Ranking)
            {
                return 1;
            }
            else if (Ranking < other.Ranking)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}
