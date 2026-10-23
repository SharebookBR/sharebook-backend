using System;
using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class UnsubscribeVM
{
    public Guid UserId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;
}
