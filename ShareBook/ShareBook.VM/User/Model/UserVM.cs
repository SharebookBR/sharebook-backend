using System;

namespace ShareBook.VM.User.Model
{
    public class UserVM
    {

        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public UserVM()
        {
            Id = new Guid();
        }
    }
}
