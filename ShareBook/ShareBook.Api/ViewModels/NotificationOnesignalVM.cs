using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class NotificationOnesignalVM
    {
        public string PlayerId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public TypeSegments TypeSegments { get; set; }
    }
}
