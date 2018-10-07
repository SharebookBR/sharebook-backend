using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class BookUserRepository : RepositoryGeneric<BookUser>, IBookUserRepository
    {
        public BookUserRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
