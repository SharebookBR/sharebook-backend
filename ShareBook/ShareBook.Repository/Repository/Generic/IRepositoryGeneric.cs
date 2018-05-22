using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Repository
{
    public interface IRepositoryGeneric<TEntity> where TEntity : class
    {
        TEntity Insert(TEntity entity);

        TEntity Update(TEntity entity);

        TEntity FindById(params object[] keyValues);

        IEnumerable<TEntity> GetAll();

        Task<TEntity> InsertAsync(TEntity entity);

        Task<TEntity> UpdateAsync(TEntity entity);

        Task<TEntity> FindByIdAsync(params object[] keyValues);

        Task<IEnumerable<TEntity>> GetAllAsync();
    }
}