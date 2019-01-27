using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels
{
    public class AddFacilitatorNotesVM
    {
        public Guid BookId { get; set; }

        public string FacilitatorNotes { get; set; }
    }
}
