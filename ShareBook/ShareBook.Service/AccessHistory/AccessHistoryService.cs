using FluentValidation;

using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;

using System.Threading.Tasks;

namespace ShareBook.Service {
    public class AccessHistoryService : BaseService<AccessHistory>, IAccessHistoryService {
        private readonly IAccessHistoryRepository _accessHistoryRepository;

        public AccessHistoryService(IAccessHistoryRepository repository,
            IUnitOfWork unitOfWork,
            IValidator<AccessHistory> validator) : base(repository, unitOfWork, validator) 
        {
            _accessHistoryRepository = repository;
        }

        public async Task InsertVisitor(User user, User visitor, VisitorProfile profile) {
            var visitorProfile = new AccessHistory(user.Id, visitor.Name, profile);

            await _accessHistoryRepository.InsertAsync(visitorProfile);
        }
    }
}