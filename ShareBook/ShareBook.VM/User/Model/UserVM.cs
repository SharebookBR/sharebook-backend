using System;
using ShareBook.VM.Common;

namespace ShareBook.VM.User.Model
{
    public class UserVM : ResultServiceVM
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Email { get; set; }

        public string Password { get; set; }
    }
}