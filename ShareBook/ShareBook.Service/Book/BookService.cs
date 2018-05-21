using System.Threading.Tasks;
using AutoMapper;
using ShareBook.Data;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Out;
using ShareBook.Data.Model;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;

namespace ShareBook.Service
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _iBookRepository;
        private readonly IUnitOfWork _iUnitOfWork;

        public BookService(IBookRepository iBookRepository,
            IUnitOfWork iUnitOfWork)
        {
            _iBookRepository = iBookRepository;
            _iUnitOfWork = iUnitOfWork;
        }
        
        public async Task<BookVM> GetBooks()
        {
            BookOut books = await _iBookRepository.GetBooks();

            return Mapper.Map<BookVM>(books);
        }

        public async Task<BookVM> GetBookById(int id)
        {
            BookOutById book = await _iBookRepository.GetBookById(id);

            return Mapper.Map<BookVM>(book);
        }

        public async Task<ResultServiceVM> CreateBook(BookVM bookVM)
        {
            Book book = Mapper.Map<Book>(bookVM);

            ResultService resultService = new ResultService(new BookValidation().Validate(book));

            _iUnitOfWork.BeginTransaction();

            if (resultService.Success)
            {
                await _iBookRepository.InsertAsync(book);
                _iUnitOfWork.Commit();
            }

            return Mapper.Map<ResultServiceVM>(resultService);
        }
    }
}
