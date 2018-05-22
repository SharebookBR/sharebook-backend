using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Service;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : Controller
    {
        private readonly IBookService _bookService;

        public BookController(IBookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IEnumerable<BookVM>> GetBooks()
        {
            /// TODO: Rever se é realmente necessário a construção desse endpoint e métodos de GetAll Books
            return await _bookService.GetBooksAsync();
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<BookVM> GetBookById(int id)
        {
            return await _bookService.GetBookByIdAsync(id);
        }

        [HttpPost]
        public async Task<ResultServiceVM> CreateBook([FromBody]BookVM bookVM)
        {
            return await _bookService.CreateBookAsync(bookVM);
        }
    }
}