using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Data;
using ShareBook.Data.Entities.User;

namespace ShareBook.Repository
{
    public class UserRepository : RepositoryGeneric<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email.Equals(email));
        }

        public async Task<User> GetByEmailAndPasswordAsync(User user)
        {
            return await _context.Users.FirstOrDefaultAsync(x => x.Email.Equals(user.Email) && x.Password.Equals(user.Password));
        }
    }
}