namespace ShareBook.Repository.Infra
{
    public interface IUnitOfWork
    {
        void SaveChanges();
    }
}