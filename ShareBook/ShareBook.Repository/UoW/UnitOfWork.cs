using System;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

namespace ShareBook.Repository.UoW;

public class UnitOfWork(ApplicationDbContext context) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;

    public Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        _context.Database.BeginTransaction(isolationLevel);
        return Task.CompletedTask;
    }
    public async Task CommitAsync() => await _context.Database.CommitTransactionAsync();
    public async Task RollbackAsync() => await _context.Database.RollbackTransactionAsync();
    public async ValueTask DisposeAsync()
    {
        if (_context?.Database?.CurrentTransaction != null)
            await _context.Database.CurrentTransaction.RollbackAsync();
    }
}
