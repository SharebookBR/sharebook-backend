using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class EmailTestVM
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string Name { get; set; }
}
