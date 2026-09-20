using FluentValidation;

using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using System.Threading.Tasks;

namespace ShareBook.Service; 
public class AccessHistoryService : BaseService<AccessHistory>, IAccessHistoryService {

    public AccessHistoryService(ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IValidator<AccessHistory> validator) : base(context, unitOfWork, validator)
    {
    }

    public async Task InsertVisitorAsync(User user, User visitor, VisitorProfile profile) {
        var visitorProfile = new AccessHistory(user.Id, visitor.Name, profile);

        await _repository.InsertAsync(visitorProfile);
    }
}