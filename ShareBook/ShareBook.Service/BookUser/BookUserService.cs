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
            _bookUserRepository.Get().Include(x => x.User).Where(x => x.BookId == bookId).Select(x => x.User.Cleanup()).ToList();

        public void Insert(Guid bookId)
        {
            var bookUser = new BookUser()
            {
                BookId = bookId,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name)
            };          

            if (!_bookService.Any(x => x.Id == bookUser.BookId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            _bookUserRepository.Insert(bookUser);
            _bookUsersEmailService.SendEmailBookRequested(bookUser);
        }

        public void DonateBook(Guid bookId, Guid userId)
        {
            var bookUserAccepted = _bookUserRepository.Get().Where(x => x.UserId == userId && x.BookId == bookId).FirstOrDefault();

            bookUserAccepted.Status = DonationStatus.Donated;

            _bookUserRepository.Update(bookUserAccepted);

            DeniedBookUsers(bookId);
        }

        public void DeniedBookUsers(Guid bookId)
        {
            var bookUsersDenied = _bookUserRepository.Get().Where(x => x.BookId == bookId).ToList();
            foreach (var item in bookUsersDenied)
            {
                item.Status = DonationStatus.Denied;
                _bookUserRepository.Update(item);
            }
        }
    }
}
