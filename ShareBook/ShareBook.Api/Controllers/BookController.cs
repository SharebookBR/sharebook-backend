using System.Collections.Generic;
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

        [HttpPost("Create")]
        public ResultServiceVM Create([FromBody]BookVM bookVM)
        {
            return _bookService.Create(bookVM);
        }

        [HttpGet]
        public IEnumerable<BookVM> Get()
        {
            return _bookService.GetAll();
        }

        [HttpGet("{id}", Name = "Get")]
        public BookVM GetById(int id)
        {
            return _bookService.GetById(id);
        }
    }
}