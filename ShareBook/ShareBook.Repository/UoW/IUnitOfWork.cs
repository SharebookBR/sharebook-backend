using System;
using System.Threading.Tasks;

namespace ShareBook.Repository.UoW;

public interface IUnitOfWork : IAsyncDisposable
{
    Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);
    Task CommitAsync();
    Task RollbackAsync();
}
