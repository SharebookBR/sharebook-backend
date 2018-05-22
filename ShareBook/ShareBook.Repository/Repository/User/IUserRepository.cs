using ShareBook.Data.Entities.User;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IUserRepository : IRepositoryGeneric<User>
    {
        Task<User> GetByEmailAndPasswordAsync(User user);

        Task<User> GetByEmailAsync(string email);
    }
}
