using Microsoft.AspNetCore.Mvc;
using ShareBook.Service;
using ShareBook.VM.Book.In;
using ShareBook.VM.Book.Out;
using ShareBook.VM.Common;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly IBookService _iBookService;

        public BookController(IBookService iBookService)
        {
            _iBookService = iBookService;
        }

        [HttpGet("GetBooks")]
        public async Task<BookOutVM> GetBooks()
        {
            return await _iBookService.GetBooks();
        }

        [HttpGet("GetBookById/{id}")]
        public async Task<BookOutByIdVM> GetBookById(int id)
        {
            return await _iBookService.GetBookById(id);
        }

        [HttpPost("CreateBook")]
        public async Task<ResultServiceVM> CreateBook([FromBody]BookInVM bookInVM)
        {
            return await _iBookService.CreateBook(bookInVM);
        }
    }
}
