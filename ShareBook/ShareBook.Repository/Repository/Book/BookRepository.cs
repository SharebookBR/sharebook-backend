using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;

namespace ShareBook.Repository
{
    public class BookRepository : RepositoryGeneric<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context) { }

        public override async Task<Book> InsertAsync(Book entity)
        {
            _context.Attach(entity.Category);
            _context.Attach(entity.User);

            _context.Add(entity);

            await _context.SaveChangesAsync();

            return entity;
        }

        public override async Task<Book> UpdateAsync(Book entity)
        {
            _context.Update(entity);

            //imagem eh opcional no update
            if (entity.ImageSlug == null)
                _context.Entry(entity).Property(x => x.ImageSlug).IsModified = false;

            if(entity.Slug == null)
                _context.Entry(entity).Property(x => x.Slug).IsModified = false;
     
            await _context.SaveChangesAsync();

            return entity;
        }

        public override async Task<PagedList<Book>> GetAsync<TKey>(Expression<Func<Book, bool>> filter, Expression<Func<Book, TKey>> order, int page, int itemsPerPage)
        {
            var skip = (page - 1) * itemsPerPage;
            var query = _dbSet.Where(filter);
            var total = await query.CountAsync();
            var result = await query.Include(x => x.BookUsers).Include(x => x.User)
                .OrderBy(order)
                .Skip(skip)
                .Take(itemsPerPage)
                .ToListAsync();

            return new PagedList<Book>()
            {
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = total,
                Items = result
            };
        }
    }
}
