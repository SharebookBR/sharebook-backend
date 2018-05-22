using System;

namespace ShareBook.Data.Entities.User
{
    public class User
    {
        public Guid Id { get;  set; }

        public string Email { get;  set; }

        public string Password { get;  set; }
    }
}
