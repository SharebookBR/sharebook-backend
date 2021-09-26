using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShareBook.Domain;

namespace ShareBook.Repository {
    public interface IBookRepository : IRepositoryGeneric<Book> {
        Task<IEnumerable<Book>> GetBooksByDonor(Guid donorId);
    }
}