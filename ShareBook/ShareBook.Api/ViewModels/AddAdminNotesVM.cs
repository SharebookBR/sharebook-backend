using System;

namespace ShareBook.Api.ViewModels;

public class AddAdminNotesVM
{
    public Guid BookId { get; set; }

    public required string AdminNotes { get; set; }
}
