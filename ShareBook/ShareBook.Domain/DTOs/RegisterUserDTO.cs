
namespace ShareBook.Domain.DTOs;

public class RegisterUserDTO
{
    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Street { get; set; }

    public string? Number { get; set; }

    public string? Complement { get; set; }

    public string? Neighborhood { get; set; }

    public string? PostalCode { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? Linkedin { get; set; }

    public string? Instagram { get; set; }

    public string? Phone { get; set; }

    public string Password { get; set; } = string.Empty;

    public bool AllowSendingEmail { get; set; } = true;

    public int Age { get; set; }

    public string? ParentEmail { get; set; }
    public string? RecaptchaReactive { get; set; }
}
