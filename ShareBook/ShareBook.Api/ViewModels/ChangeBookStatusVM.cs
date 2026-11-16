using System.ComponentModel.DataAnnotations;
using ShareBook.Domain.Enums;

namespace ShareBook.Api.ViewModels;

public class ChangeBookStatusVM
{
    [Required]
    [EnumDataType(typeof(BookStatus))]
    public BookStatus? NewStatus { get; set; }

    [Required]
    [StringLength(600, MinimumLength = 3)]
    public required string Reason { get; set; }
}
