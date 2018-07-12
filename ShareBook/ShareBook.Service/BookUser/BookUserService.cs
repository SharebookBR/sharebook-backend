using System;
using System.Threading;
using ShareBook.Domain;
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

        public void Insert(Guid idBook)
        {
            var bookUser = new BookUser()
            {
                BookId = idBook,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name)
            };          

            if (!_bookService.Any(x => x.Id == bookUser.BookId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            _bookUserRepository.Insert(bookUser);
            _bookUsersEmailService.SendEmailBookRequested(bookUser);
        }
    }
}
