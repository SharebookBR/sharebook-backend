using ShareBook.Domain;

namespace ShareBook.Repository
{
    public interface IUserRepository : IRepositoryGeneric<User>
    {
        User UpdatePassword(User user);
    }
}
