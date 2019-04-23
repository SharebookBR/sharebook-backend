using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;

namespace ShareBook.Domain
{
    public class NotificationOnesignal
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public TypeSegments TypeSegments { get; set; }
        public string UrlImage { get; set; }
        public IList<string> LanguageCodes { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }


    
}
