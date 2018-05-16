using ShareBook.Data;
using ShareBook.Data.Model;

namespace ShareBook.Repository.Repository
{
    public class UserRepository : RepositoryGeneric<User>,  IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
       : base(context)
        {
            _context = context;
        }
    }
}
