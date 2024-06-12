using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IUserRepository : IRepositoryGeneric<User>
    {
        Task<User> UpdatePasswordAsync(User user);
    }
}
