using ShareBook.Domain;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Repository {
    public interface IBookUserRepository : IRepositoryGeneric<BookUser> {
        Task<IEnumerable<BookUser>> GetDonationRequestsByRequester(Guid requesterId);
        Task<IEnumerable<BookUser>> GetDonationRequestsByDonor(Guid donorId);
    }
}