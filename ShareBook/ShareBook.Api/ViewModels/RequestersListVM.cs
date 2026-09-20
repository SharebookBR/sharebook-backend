using System;

namespace ShareBook.Api.ViewModels;

public class RequestersListVM
{
    public Guid UserId { get; set; }

    public string RequesterNickName { get; set; } = string.Empty;

    public string? Location { get; set; }

    public int TotalBooksWon { get; set; }

    public int TotalBooksDonated { get; set; }

    public string? RequestText { get; set; }

    public string Status { get; set; } = string.Empty;

}
