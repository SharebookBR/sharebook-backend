using ShareBook.VM.Common;
using System;

namespace ShareBook.VM.User.Model
{
    public class UserVM : ResultServiceVM
    {

        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public UserVM()
        {
            Id = Guid.NewGuid();
        }
    }
}
