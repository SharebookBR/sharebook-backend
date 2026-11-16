using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class ChooseAnotherWinnerVM
{
    [Required]
    [StringLength(600, MinimumLength = 3)]
    public required string Reason { get; set; }
}
