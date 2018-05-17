using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.VM.User.Model
{
    public class UserVM
    {

        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
