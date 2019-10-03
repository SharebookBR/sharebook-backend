using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.Muambator
{
    public class MuambatorDTO
    {
        public string Status { get; set; }

        public string Message { get; set; }

        public IList<dynamic> Results { get; set; }
    }
}
