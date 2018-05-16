using System.Threading.Tasks;
using AutoMapper;
using ShareBook.Data;
using ShareBook.Data.Common;
using ShareBook.Data.Entities.Book.Out;
using ShareBook.Data.Model;
using ShareBook.Repository;
using ShareBook.Repository.Infra;
using ShareBook.VM.Book.In;
using ShareBook.VM.Book.Out;
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
        
        public async Task<BookOutVM> GetBooks()
        {
            BookOut books = await _iBookRepository.GetBooks();

            return Mapper.Map<BookOutVM>(books);
        }

        public async Task<BookOutByIdVM> GetBookById(int id)
        {
            BookOutById book = await _iBookRepository.GetBookById(id);

            return Mapper.Map<BookOutByIdVM>(book);
        }

        public async Task<ResultServiceVM> CreateBook(BookInVM bookInVM)
        {
            Book book = Mapper.Map<Book>(bookInVM);

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
