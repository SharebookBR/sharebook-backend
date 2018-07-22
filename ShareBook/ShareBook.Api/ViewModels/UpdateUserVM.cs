using Newtonsoft.Json;
using ShareBook.Domain.Enums;
using System;

namespace ShareBook.Api.ViewModels
{
    public class UpdateUserVM : BaseViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Linkedin { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
    }
}
