using ShareBook.Domain;

namespace ShareBook.Repository {
    public class UserCancellationInfoRepository : RepositoryGeneric<UserCancellationInfo>, IUserCancellationInfoRepository {
        public UserCancellationInfoRepository(ApplicationDbContext context) : base(context) { }
    }
}