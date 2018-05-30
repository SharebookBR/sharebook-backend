using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public Profile Profile { get;  set; } = Profile.User;
       
    }
}
