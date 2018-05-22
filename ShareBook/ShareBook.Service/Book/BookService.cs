using System.Collections.Generic;
using AutoMapper;
using ShareBook.Data;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;

namespace ShareBook.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IBookRepository bookRepository,
            IUnitOfWork unitOfWork)
        {
            _bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
        }

        public ResultServiceVM Create(BookVM bookVM)
        {
            var book = Mapper.Map<Book>(bookVM);

            var resultService = new ResultService(new BookValidation().Validate(book));

            if (resultService.Success)
            {
                _bookRepository.Insert(book);
                _unitOfWork.SaveChanges();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }

        public IEnumerable<BookVM> GetAll()
        {
            var books = _bookRepository.GetAll();

            return Mapper.Map<IEnumerable<BookVM>>(books);
        }

        public BookVM GetById(int id)
        {
            var book = _bookRepository.GetById(id);

            return Mapper.Map<BookVM>(book);
        }
    }
}