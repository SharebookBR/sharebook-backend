using System;

namespace ShareBook.Domain.DTOs;

public class BookCancelationDTO
{
    public Book Book { get; set; } = null!;
    public string CanceledBy { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
