using ShareBook.Data.Common;
using ShareBook.Data.Entities.User.Model;
namespace ShareBook.Data.Entities.User.Out
{
    public class UserOutById : ResultService
    {
        public UserModel User { get; set; }
    }
}
