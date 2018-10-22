using System;

namespace ShareBook.Api.ViewModels
{
    public class MyBookRequestVM
    {
        public Guid RequestId { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Status { get; set; }
    }
}
