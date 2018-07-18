using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;
using System.Collections.Generic;

namespace ShareBook.Service
{
    public interface IUserService : IBaseService<User>
    {
        Result<User> AuthenticationByEmailAndPassword(User user);
        IEnumerable<User> GetAllAdministrators();
        Result<User> Update(User user);
    }
}
