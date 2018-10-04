using ShareBook.Domain.Entities;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IUserRepository : IRepositoryGeneric<User>
    {
        Task<User> UpdatePassword(User user);
    }
}
