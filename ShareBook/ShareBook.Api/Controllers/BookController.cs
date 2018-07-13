using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : BaseController<Book>
    {
        private readonly IBookUserService _bookUserService;

        public BookController(IBookService bookService, IBookUserService bookUserService) : base(bookService)
        {
            _bookUserService = bookUserService;
            SetDefault(x => x.Title);
        }

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.AprovarLivro)]
        public Result<Book> Approve(string id) => ((IBookService)_service).Approve(new Guid(id));

        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = ((IBookService)_service).FreightOptions();
            return freightOptions;
        }

        [HttpGet("Top15NewBooks")]
        public IList<Book> Top15NewBooks() => ((IBookService)_service).Top15NewBooks();

        [HttpGet("Random15Books")]
        public IList<Book> Random15Books() => ((IBookService)_service).Random15Books();

        [Authorize("Bearer")]
        [HttpPost("Request/{id}")]
        public IActionResult RequestBook(string id)
        {         
            _bookUserService.Insert(new Guid(id));
            return Ok();
        }

        [Authorize("Bearer")]
        [HttpGet("ByTitle/{title}")]
        public IList<Book> ByTitle(string title) => ((IBookService)_service).ByTitle(title);

        [Authorize("Bearer")]
        [HttpGet("ByAuthor/{author}")]
        public IList<Book> ByAuthor(string author) => ((IBookService)_service).ByAuthor(author);

        [Authorize("Bearer")]
        [HttpGet("{page}/{items}")]
        public override PagedList<Book> Paged(int page, int items)
        {
            return ((IBookService)_service).GetAll(page, items);
        }

        [Authorize("Bearer")]
        [HttpGet()]
        public override PagedList<Book>GetAll()
        {
            return ((IBookService)_service).GetAll(1, 15);
        }
    }
}
