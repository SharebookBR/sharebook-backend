using Newtonsoft.Json;
using ShareBook.Domain;
using System.ComponentModel.DataAnnotations;

namespace ShareBook.Api.ViewModels;

public class UpdateUserVM : BaseViewModel
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Email { get; set; }

    public string? Linkedin { get; set; }

    public string? Instagram { get; set; }

    public Address? Address { get; set; }

    public string? Phone { get; set; }

    public bool AllowSendingEmail { get; set; }
}
