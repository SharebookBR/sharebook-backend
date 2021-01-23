using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels
{
    public class UpdateBookVM : BaseViewModel
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public Guid CategoryId { get; set; }

        public Guid UserId { get; set; }

        public Guid UserIdFacilitator { get; set; }

        public bool Approved { get; set; }

        public string ImageName { get; set; }

        public byte[] ImageBytes { get; set; }

        public string Synopsis { get; set; }

        public FreightOption FreightOption { get; set; }
        public string Type { get; set; }
        public string EBookDownloadLink { get; set; }
        public string EBookPdfFile { get; set; }
    }
}
