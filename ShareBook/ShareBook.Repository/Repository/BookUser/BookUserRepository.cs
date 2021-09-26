using ShareBook.Domain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain.Enums;

namespace ShareBook.Repository {
    public class BookUserRepository : RepositoryGeneric<BookUser>, IBookUserRepository {
        private readonly DbSet<Book> _booksDb;
        public BookUserRepository(ApplicationDbContext context) : base(context) {
            _booksDb = context.Books;
        }
        
        /// <summary>
        /// Obtem os pedidos de doação por requisitante
        /// </summary>
        /// <param name="requesterId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BookUser>> GetDonationRequestsByRequester(Guid requesterId) {
            var ordersByRequester = from booksUser in _dbSet.Include(bu => bu.Book)
                where booksUser.UserId.Equals(requesterId) && booksUser.Status.Equals(DonationStatus.WaitingAction)
                select booksUser;

            return await ordersByRequester.ToListAsync();
        }

        /// <summary>
        /// Obtem os pedidos de doação por doador
        /// </summary>
        /// <param name="donorId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<BookUser>> GetDonationRequestsByDonor(Guid donorId) {
            var ordersByDonor = from myBook in _booksDb
                join bookUsers in _dbSet.Include(bu => bu.Book) on myBook equals bookUsers.Book
                where myBook.UserId.Equals(donorId)
                      && (myBook.Status.Equals(BookStatus.Available)
                          || myBook.Status.Equals(BookStatus.AwaitingDonorDecision)
                          || myBook.Status.Equals(BookStatus.WaitingApproval))
                select bookUsers;

            return await ordersByDonor.ToListAsync();
        }
    }
}