using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class UpdateUserVM
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string OldPassword { get; set; }
        public string Linkedin { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public Profile Profile { get; set; } = Profile.User;
    }
}
