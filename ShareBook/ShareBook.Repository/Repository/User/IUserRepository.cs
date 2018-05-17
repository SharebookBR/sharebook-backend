using ShareBook.Data.Entities.User.Out;
using ShareBook.Data.Model;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IUserRepository : IRepositoryGeneric<User>
    {
        Task<UserOutById> GetByEmailAndPasswordAsync(User user);
    }
}
