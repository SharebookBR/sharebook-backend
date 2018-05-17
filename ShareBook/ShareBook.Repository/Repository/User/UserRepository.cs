using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Data;
using ShareBook.Data.Entities.User;

namespace ShareBook.Repository
{
    public class UserRepository : RepositoryGeneric<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
       : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAndPasswordAsync(User user)
        {
            user = await _context.Users.Where(e => e.Email == user.Email && e.Password == user.Password).Select(x => new User
            {
                Id = x.Id,
                Email = x.Email,

            }).FirstOrDefaultAsync();

            return user;
        }
    }
}
