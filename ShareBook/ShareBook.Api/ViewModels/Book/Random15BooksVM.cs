using System;

namespace ShareBook.Api.ViewModels
{
    public class Random15BooksVM
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string Author { get; set; }

        public string ImageSlug { get; set; }

        public string ImageUrl { get; set; }

        public bool Approved { get; set; }
    }
}
