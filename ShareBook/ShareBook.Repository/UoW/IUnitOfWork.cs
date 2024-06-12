using System;
using System.Threading.Tasks;

namespace ShareBook.Repository.UoW
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
