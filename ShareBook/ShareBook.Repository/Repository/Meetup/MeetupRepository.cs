using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Repository;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public class MeetupRepository : RepositoryGeneric<Meetup>, IMeetupRepository
    {
        public MeetupRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<PagedList<Meetup>> GetAsync<TKey>(Expression<Func<Meetup, bool>> filter, Expression<Func<Meetup, TKey>> order, int page, int itemsPerPage, IncludeList<Meetup> includes)
        {
            var skip = (page - 1) * itemsPerPage;
            var query = _dbSet.AsQueryable();

            query = query.Where(filter);
            var total = await query.CountAsync();
            var result = await query
                .OrderByDescending(order)
                .Skip(skip)
                .Take(itemsPerPage)
                .ToListAsync();

            return new PagedList<Meetup>()
            {
                Page = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = total,
                Items = result
            };
        }
    }
}