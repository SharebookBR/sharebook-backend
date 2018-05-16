using ShareBook.Data;
namespace ShareBook.Repository.Infra
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public void BeginTransaction()
        {
            _context.Database.BeginTransaction();
        }

        public bool Commit()
        {
            _context.Database.CommitTransaction();
            return true;
        }

        public void Rollback()
        {
            _context.Database.RollbackTransaction();
        }
    }
}
