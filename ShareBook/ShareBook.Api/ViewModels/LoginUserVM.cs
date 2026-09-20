using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class LoginUserVM
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
