using System;

namespace MultiTool.DTO
{
    public class UrlHistoryViewModel : IEquatable<UrlHistoryViewModel>
    {
        public DateTime Date { get; set; }
        public string Link { get; set; }

        public bool Equals(UrlHistoryViewModel other)
        {
            return Date.Equals(other.Date) && Link.Equals(other.Link);
        }
    }
}
