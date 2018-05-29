using ShareBook.Domain.Common;
using System;

namespace ShareBook.Domain
{
    public class LogEntry : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string EntityName { get; set; }
        public Guid EntityId { get; set; }
        public string Operation { get; set; }
        public DateTime LogDateTime { get; set; }
        public string ValuesChanges { get; set; }
    }
}
