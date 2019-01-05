using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sharebook.Jobs
{
    public class GenericJob
    {
        public string JobName { get; set; }
        public string Description { get; set; }
        public Interval Interval { get; set; }
        public bool Active { get; set; }
    }
}
