using System;
using System.Threading;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.CustomExceptions;

namespace ShareBook.Service
{
    public class BookUserService : IBookUserService
    {

        private readonly IBookUserRepository _bookUserRepository;
        private readonly IBookRepository _bookRepository;

        public BookUserService(IBookUserRepository bookUserRepository, IBookRepository bookRepository, IUnitOfWork unitOfWork)
        {
            _bookUserRepository = bookUserRepository;
            _bookRepository = bookRepository;
        }

        public void Insert(Guid idBook)
        {
            var bookUser = new BookUser()
            {
                BookId = idBook,
                UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name)
            };          

            if (!_bookRepository.Any(x => x.UserId == bookUser.UserId))
                throw new ShareBookException(ShareBookException.Error.NotFound);

            if (_bookUserRepository.Any(x => x.UserId == bookUser.UserId && x.BookId == bookUser.BookId))
                throw new ShareBookException("O usuário já possui uma requisição para o mesmo livro.");

            _bookUserRepository.Insert(bookUser);
        }
    }
}
