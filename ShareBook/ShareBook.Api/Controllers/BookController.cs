using Microsoft.AspNetCore.Mvc;
using ShareBook.Service;
using ShareBook.VM.Book.Model;
using ShareBook.VM.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            return await _bookService.GetBooks();
        }

        [HttpGet("{id}", Name = "Get")]
        public async Task<BookVM> GetBookById(int id)
        {
            return await _bookService.GetBookById(id);
        }

        [HttpPost]
        public async Task<ResultServiceVM> CreateBook([FromBody]BookVM bookVM)
        {
            return await _bookService.CreateBook(bookVM);
        }
    }
}
