namespace ShareBook.Repository.Infra
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context) => _context = context;

        public void BeginTransaction() => _context.Database.BeginTransaction();
        public void Commit() => _context.Database.CommitTransaction();
        public void Rollback() => _context.Database.RollbackTransaction();

        ~UnitOfWork() => _context.Database.CurrentTransaction?.Rollback();
    }
}
