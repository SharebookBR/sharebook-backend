using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Domain.Common
{
    public class JobExecutorResult
    {
        public bool Success { get; set; }
        public IList<string> Messages { get; set; }
    }
}
