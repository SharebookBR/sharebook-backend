using System.Threading.Tasks;
using ShareBook.Domain;
using ShareBook.Domain.Enums;

namespace ShareBook.Service {
    public interface IAccessHistoryService
    {
        Task InsertVisitorAsync(User user, User visitor, VisitorProfile profile);
    }
}