using System;

namespace ShareBook.Api.ViewModels
{
    public class EbookComplaintVM : BaseViewModel
    {
        public Guid UserId { get; set; }
        public Guid BookId { get; set; }
        public string ReasonMessage { get; set; }
    }
}
