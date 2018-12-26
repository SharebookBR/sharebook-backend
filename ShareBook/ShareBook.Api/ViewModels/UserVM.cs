using ShareBook.Domain;
using System;

namespace ShareBook.Api.ViewModels
{
    public class UserVM : BaseViewModel
    {

        public string Name { get; set; }

        public string Email { get; set; }

        public string Linkedin { get; set; }

        public string Phone { get; set; }

        public Address Address { get; set; }
    }

    public class UserFacilitatorVM
    {

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Linkedin { get; set; }

        public string Phone { get; set; }

        public Address Address { get; set; }
    }
}
