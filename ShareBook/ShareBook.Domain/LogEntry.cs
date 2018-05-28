using ShareBook.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Domain
{
    public class LogEntry : BaseEntity
    {
        public Guid UserId { get; set; }
        public string EntityName { get; set; }
        public Guid EntityId { get; set; }
        public string Operation { get; set; }
        public DateTime LogDateTime { get; set; }
        public string OriginalValues { get; set; }
        public string UpdatedValues { get; set; }
    }
}
