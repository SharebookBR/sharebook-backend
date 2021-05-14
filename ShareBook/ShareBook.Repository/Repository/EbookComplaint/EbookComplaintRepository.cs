using ShareBook.Domain;

namespace ShareBook.Repository
{
    public class EbookComplaintRepository : RepositoryGeneric<EbookComplaint>, IEbookComplaintRepository
    {
        public EbookComplaintRepository(ApplicationDbContext context) : base(context) { }
    }
}