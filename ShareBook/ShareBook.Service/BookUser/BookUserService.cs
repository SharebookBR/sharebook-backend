using System;
using System.Linq.Expressions;
using System.Threading;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.CustomExceptions;
using ShareBook.Service.Generic;

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
                BookId = idBook
            };

            bookUser.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            if (_bookRepository.Get(bookUser.BookId) == null)
                throw new ShareBookException(ShareBookException.Error.NotFound);

            _bookUserRepository.Insert(bookUser);
        }
    }
}
