using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels
{
    public class CreateBookVM : BaseViewModel
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public Guid CategoryId { get; set; }

        public string ImageName { get; set; }

        public byte[] ImageBytes { get; set; }

        public FreightOption? FreightOption { get; set; }

        public string Synopsis { get; set; }

        public string Type { get; set; } = "Printed";

        public byte[] PdfBytes { get; set; }
    }
}
