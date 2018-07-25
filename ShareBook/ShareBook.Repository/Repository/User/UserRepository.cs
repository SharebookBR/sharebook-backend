using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class UserRepository : RepositoryGeneric<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public User UpdatePassword(User user)
        {
            _dbSet.Update(user);
            _context.Entry(user).Property(x => x.Password).IsModified = true;
            _context.Entry(user).Property(x => x.PasswordSalt).IsModified = true;
            _context.SaveChanges();
            return user;
        }
    }
}
