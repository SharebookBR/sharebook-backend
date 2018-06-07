using System;
using System.Threading;
using FluentValidation;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Authorization;
using ShareBook.Service.CustomExceptions;
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
                throw new ShareBookException(ShareBookException.Error.NotFound);

            book.Approved = true;
            _repository.Update(book);

            return new Result<Book>(book);
        }

        public override Result<Book> Insert(Book entity)
        {
            var result = Validate(entity);

            entity.UserId = new Guid(Thread.CurrentPrincipal?.Identity?.Name);

            //TODO - Criar módulo para upload do imageBytes

            if(result.Success)
                  result.Value = _repository.Insert(entity);

            return result;
        }
    }
}
