using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<BookVM>> GetBooksAsync()
        {
            /// TODO: Refatorar e remover a referência do "Microsoft.EntityFrameworkCore" da camada Service
            return await _bookRepository.GetBooks().AsQueryable().ProjectTo<BookVM>().ToListAsync();
        }

        public async Task<BookVM> GetBookByIdAsync(int id)
        {
            Book book = await _bookRepository.GetBookByIdAsync(id);

            return Mapper.Map<BookVM>(book);
        }

        public async Task<ResultServiceVM> CreateBookAsync(BookVM bookVM)
        {
            Book book = Mapper.Map<Book>(bookVM);

            ResultService resultService = new ResultService(new BookValidation().Validate(book));

            _unitOfWork.BeginTransaction();

            if (resultService.Success)
            {
                await _bookRepository.InsertAsync(book);
                _unitOfWork.Commit();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }
    }
}