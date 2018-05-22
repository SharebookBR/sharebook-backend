using ShareBook.Data.Entities.User;

namespace ShareBook.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        User GetByEmail(string email);

        User GetByEmailAndPassword(User user);
    }
}