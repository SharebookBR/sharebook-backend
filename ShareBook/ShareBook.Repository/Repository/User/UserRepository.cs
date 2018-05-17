using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Data;
using ShareBook.Data.Entities.User.Model;
using ShareBook.Data.Entities.User.Out;
using ShareBook.Data.Model;

namespace ShareBook.Repository
{
    public class UserRepository : RepositoryGeneric<User>,  IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
       : base(context)
        {
            _context = context;
        }

        public async Task<UserOutById> GetByEmailAndPasswordAsync(User user)
        {
            UserOutById userOutById = new UserOutById
            {
                User = await _context.Users.Where(e => e.Email == user.Email && e.Password == user.Password).Select(x => new UserModel
                {
                    Id = x.Id,
                    Email = x.Email,

                }).FirstOrDefaultAsync()
            };

            return userOutById;
        }
    }
}
