using Microsoft.EntityFrameworkCore;

using ShareBook.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Repository;
public class AccessHistoryRepository(ApplicationDbContext context) : IAccessHistoryRepository {
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<AccessHistory>> GetWhoAccessedMyProfileAsync(Guid userId) {
        if (userId.Equals(null)) return null;

        var list = from u in _context.AccessHistories
            where (u.UserId.Equals(userId))
            select u;

        return await list.ToListAsync();
    }
}
