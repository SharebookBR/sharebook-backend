using ShareBook.Domain.Common;

namespace ShareBook.Domain
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
    }
}
