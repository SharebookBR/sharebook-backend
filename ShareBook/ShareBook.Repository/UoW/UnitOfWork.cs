using System;
using System.Threading.Tasks;

namespace ShareBook.Repository.UoW
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public async Task CommitAsync() => await _context.Database.CommitTransactionAsync();
        public async Task RollbackAsync() => await _context.Database.RollbackTransactionAsync();
        public async ValueTask DisposeAsync() => await _context.Database.CurrentTransaction?.RollbackAsync();
    }
}
