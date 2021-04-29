using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Domain.DTOs
{
    public class BookTotalStatusDTO
    {
        public int TotalWaitingApproval { get; set; }
        public int TotalLate { get; set; }
        public int TotalOk { get; set; }
    }
}
