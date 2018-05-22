using ShareBook.Data;
using ShareBook.Data.Entities.User;

namespace ShareBook.Repository
{
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public User GetByEmail(string email)
        {
            return SingleOrDefault(x => x.Email.Equals(email));
        }

        public User GetByEmailAndPassword(User user)
        {
            return SingleOrDefault(x => x.Email.Equals(user.Email) && x.Password.Equals(user.Password));
        }
    }
}