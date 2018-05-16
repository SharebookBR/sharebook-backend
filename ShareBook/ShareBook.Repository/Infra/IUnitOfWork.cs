namespace ShareBook.Repository.Infra
{
    public interface IUnitOfWork
    {
        void BeginTransaction();
        bool Commit();
        void Rollback();
    }
}
