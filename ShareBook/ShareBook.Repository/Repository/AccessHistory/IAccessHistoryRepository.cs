using ShareBook.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Repository;
public interface IAccessHistoryRepository {
    Task<IEnumerable<AccessHistory>> GetWhoAccessedMyProfileAsync(Guid userId);
}
