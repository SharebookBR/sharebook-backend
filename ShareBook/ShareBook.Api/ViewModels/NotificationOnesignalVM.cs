using ShareBook.Domain.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels
{
    public class NotificationOnesignalVM
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public TypeSegments TypeSegments { get; set; }
        public string UrlImage { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string Value { get; set; }
    }
}
