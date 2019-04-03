using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ShareBook.Domain
{
    public class NotificationOnesignal
    {
        public string PlayerId { get; set; }
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public TypeSegments TypeSegments { get; set; }
        public string Url { get; set; }
        public IList<string> Language { get; set; }
    }
}
