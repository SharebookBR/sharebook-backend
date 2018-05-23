using FluentValidation;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.Service.Generic;

namespace ShareBook.Service
{
    public class BookService : BaseService<Book>, IBookService
    {
        public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork, IValidator<Book> validator) : base(bookRepository, unitOfWork, validator) { }
    }
}
