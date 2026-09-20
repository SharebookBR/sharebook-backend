using ShareBook.Domain;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;

public interface IUserRepository
{
    IQueryable<User> Get();

    Task<User> UpdatePasswordAsync(User user);
}
