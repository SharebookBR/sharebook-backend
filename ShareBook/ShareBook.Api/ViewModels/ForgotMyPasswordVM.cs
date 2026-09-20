using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class ForgotMyPasswordVM
{
    [Required]
    public required string Email { get; set; }
}
