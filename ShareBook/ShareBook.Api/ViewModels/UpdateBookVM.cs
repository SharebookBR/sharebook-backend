using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class UpdateBookVM : BaseViewModel
    {
        public string Title { get; set; }

        public string Author { get; set; }

        public Guid CategoryId { get; set; }

        public bool Approved { get; set; }

        public string ImageName { get; set; }

        public byte[] ImageBytes { get; set; }

        public FreightOption FreightOption { get; set; }
    }
}
