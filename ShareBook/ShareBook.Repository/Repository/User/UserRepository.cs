using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class UserRepository : RepositoryGeneric<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
