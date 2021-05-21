using System;

namespace ShareBook.Domain.DTOs
{
    public class UserStatsDTO
    {
        public DateTime? CreationDate { get; set; }
        public int TotalLate { get; set; }
        public int TotalOk { get; set; }
        public int TotalCanceled { get; set; }
    }
}
