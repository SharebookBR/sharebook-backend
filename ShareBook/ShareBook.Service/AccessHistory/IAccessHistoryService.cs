using System.Threading.Tasks;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Service.Generic;

namespace ShareBook.Service {
    public interface IAccessHistoryService
    {
        Task InsertVisitor(User user, User visitor, VisitorProfile profile);
    }
}