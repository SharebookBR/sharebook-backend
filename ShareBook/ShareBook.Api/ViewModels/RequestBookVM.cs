using System;
namespace ShareBook.Api.ViewModels
{
    public class RequestBookVM
    {
        public Guid BookId { get; set; }

        public string Reason { get; set; }
    }
}
