using ShareBook.Domain;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public interface IUserService : IBaseService<User>
    {
        User GetByEmailAndPassword(User user);
    }
}
