using System;

namespace ShareBook.Api.ViewModels;

public class CancelBookDonationVM
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
