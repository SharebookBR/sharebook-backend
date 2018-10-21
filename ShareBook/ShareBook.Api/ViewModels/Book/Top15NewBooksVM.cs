using Microsoft.Extensions.Options;
using ShareBook.Service.Server;
using ShareBook.Service.Upload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class Top15NewBooksVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string Author { get; set; }

        public string ImageSlug { get; set; }

        public string ImageUrl { get; set; }

        public bool Approved { get; set; }
    }
}
