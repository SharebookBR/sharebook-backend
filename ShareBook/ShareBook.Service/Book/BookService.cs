using System;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Authorization;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork, IValidator<Book> validator) : base(bookRepository, unitOfWork, validator) { }

        [AuthorizationInterceptor(Permissions.Permission.AprovarLivro)]
        public Result<Book> Approve(Guid bookId)
        {
            var book = _repository.Get(bookId);
            if (book == null)
                throw new IndexOutOfRangeException("Livro não encontrado.");

            book.Approved = true;
            _repository.Update(book);

            return new Result<Book>(book);
        }
    }
}
