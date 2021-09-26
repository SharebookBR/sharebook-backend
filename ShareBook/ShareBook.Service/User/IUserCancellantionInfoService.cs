using ShareBook.Domain;

using System.Threading.Tasks;

namespace ShareBook.Service {
    public interface IUserCancellantionInfoService {
        Task<bool> ToProceed(UserCancellationInfo userCancelInfo);
    }
}