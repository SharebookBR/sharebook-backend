using ShareBook.Data.Common;


namespace ShareBook.Data.Entities.User.In
{
    public class UserIn : ResultService
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
