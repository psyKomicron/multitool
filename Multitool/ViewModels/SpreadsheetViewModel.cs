using System;

namespace MultitoolWPF.ViewModels
{
    public class SpreadsheetViewModel : IComparable<SpreadsheetViewModel>
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Ranking { get; set; }

        public int CompareTo(SpreadsheetViewModel other)
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
