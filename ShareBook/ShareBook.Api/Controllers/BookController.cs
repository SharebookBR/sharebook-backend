using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;

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

        [HttpGet()]
        public PagedList<Book> GetBooks() => GetBooksPaged(1, 15);

        [HttpGet("{page}/{items}")]
        public PagedList<Book> GetBooksPaged(int page, int items) => _bookService.Get(x => true, x => x.Name, page, items);

        [HttpGet("{id}", Name = "Get")]
        public Book GetBookById(int id) => _bookService.Get(id);

        [Authorize("Bearer")]
        [HttpPost]
        public Result<Book> CreateBook([FromBody]Book book) => _bookService.Insert(book);
    }
}
