using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Data.Entities.User.Model
{
    public class UserModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
    }
}
