using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class BookRepository : RepositoryGeneric<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Book> UpdateAsync(Book entity)
        {
            _context.Update(entity);

            //A imagem será atualizada na V2 do Sharebook
            _context.Entry(entity).Property(x => x.ImageSlug).IsModified = false;

            _context.Entry(entity).Property(x => x.UserId).IsModified = false;

            await _context.SaveChangesAsync();

            return entity;
        }
    }
}
