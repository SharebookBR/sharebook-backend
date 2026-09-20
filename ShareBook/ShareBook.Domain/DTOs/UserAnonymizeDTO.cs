using System;

namespace ShareBook.Domain.DTOs;

public class UserAnonymizeDTO
{
    public Guid UserId { get; set; }
    public string Password { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
