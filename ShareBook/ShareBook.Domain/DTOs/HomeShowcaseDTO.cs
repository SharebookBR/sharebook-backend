using System;
using System.Collections.Generic;

namespace ShareBook.Domain.DTOs
{
    public class HomeShowcaseBookDTO
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Slug { get; set; }
        public string ImageUrl { get; set; }
        public string Type { get; set; }
    }

    public class HomeShowcaseCategoryDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<HomeShowcaseBookDTO> Books { get; set; } = new();
    }
}
