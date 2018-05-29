using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;

namespace ShareBook.Domain
{
    public class User : BaseEntity
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsAdministrator { get; private set; } = false;
        
        public void UpdateUserRole(bool isAdmin)
        {
            IsAdministrator = isAdmin;
        }

        public string GetProfile()
        {
            return IsAdministrator ? EProfile.ADMINISTRATOR.ToString() : EProfile.USER.ToString();
        }
    }
}
