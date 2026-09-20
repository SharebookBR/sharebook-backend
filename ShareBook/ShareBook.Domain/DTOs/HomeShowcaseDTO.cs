using System;
using System.Collections.Generic;

namespace ShareBook.Domain.DTOs;

public class HomeShowcaseBookDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class HomeShowcaseCategoryDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<HomeShowcaseBookDTO> Books { get; set; } = new();
}
