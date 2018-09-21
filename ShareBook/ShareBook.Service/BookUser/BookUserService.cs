using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Repository.Infra;

namespace ShareBook.Service
{
    public class BookUserService : IBookUserService
    {

        private readonly IBookUserRepository _bookUserRepository;
        private readonly IBookService _bookService;
        private readonly IBookUsersEmailService _bookUsersEmailService;

        public BookUserService(IBookUserRepository bookUserRepository, IBookService bookService, 
            IBookUsersEmailService bookUsersEmailService, IUnitOfWork unitOfWork)
        {
            _bookUserRepository = bookUserRepository;
            _bookService = bookService;
            _bookUsersEmailService = bookUsersEmailService;
        }

        public IList<User> GetGranteeUsersByBookId(Guid bookId) =>
            _bookUserRepository.Get().Include(x => x.User)
            .Where(x => x.BookId == bookId && x.Status == DonationStatus.WaitingAction)
            .Select(x => x.User.Cleanup()).ToList();

        public void Insert(Guid bookId, string reason)
        {
            var bookUser = new BookUser()
            {
                BookId = bookId,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name),
                Reason = reason
            };          

            if (!_bookService.Any(x => x.Id == bookUser.BookId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            _bookUserRepository.Insert(bookUser);
             _bookUsersEmailService.SendEmailBookRequested(bookUser);
        }

        public async void DonateBook(Guid bookId, Guid userId, string note)
        {
            var bookUserAccepted = _bookUserRepository.Get().Include(u => u.Book).Include( u => u.User)
                .Where(x => x.UserId == userId 
                    && x.BookId == bookId 
                    && x.Status == DonationStatus.WaitingAction)
                    .FirstOrDefault();

            if(bookUserAccepted == null) 
                throw new ShareBookException("Não existe a relação de usuário e livro para a doação.");

            bookUserAccepted.UpdateBookUser(DonationStatus.Donated, note);

            _bookUserRepository.Update(bookUserAccepted);

            DeniedBookUsers(bookId);

            _bookService.Approve(bookId, false);

            await _bookUsersEmailService.SendEmailBookDonated(bookUserAccepted);
        }

        public void DeniedBookUsers(Guid bookId)
        {
            var bookUsersDenied = _bookUserRepository.Get().Where(x => x.BookId == bookId
            && x.Status == DonationStatus.WaitingAction).ToList();
            foreach (var item in bookUsersDenied)
            {
                string note = string.Empty;
                item.UpdateBookUser(DonationStatus.Denied, note);
                _bookUserRepository.Update(item);
            }
        }

        public IList<BookUser> GetRequestsByUser()
        {
            var userId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return _bookUserRepository.Get().Include(u => u.Book)
                            .Where(x => x.UserId == userId).ToList();
        }
    }
}
