using System;

namespace ShareBook.Api.ViewModels;

public class AddFacilitatorNotesVM
{
    public Guid BookId { get; set; }

    public required string FacilitatorNotes { get; set; }
}
