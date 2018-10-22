using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public interface IUserService : IBaseService<User>
    {
        Result<User> AuthenticationByEmailAndPassword(User user);
        new Result<User> Update(User user);
        Result<User> ChangeUserPassword(User user, string oldPassword);
    }
}
