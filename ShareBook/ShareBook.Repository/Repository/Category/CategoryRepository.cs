using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class CategoryRepository : RepositoryGeneric<Category>,  ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context) { }
    }
}
