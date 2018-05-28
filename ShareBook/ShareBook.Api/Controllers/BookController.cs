using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : BaseController
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
        public Book GetBookById(string id) => _bookService.Get(new Guid(id));

        [Authorize("Bearer")]
        [HttpPost]
        public Result<Book> CreateBook([FromBody]Book book) => _bookService.Insert(book);

        [Authorize("Bearer")]
        [HttpPut]
        public Result<Book> UpdateBook([FromBody]Book book) => _bookService.Update(book);

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        public Result UpdateBook(string id) => _bookService.Delete(new Guid(id));
    }
}
