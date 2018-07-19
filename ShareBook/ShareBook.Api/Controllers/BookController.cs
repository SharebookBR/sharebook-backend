using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using ShareBook.Service.Authorization;
using System;
using System.Collections.Generic;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BookController : Controller
    {
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _bookService;

        public BookController(IBookService bookService, IBookUserService bookUserService) 
        {
            _bookService = bookService;
            _bookUserService = bookUserService;
        }

        [Authorize("Bearer")]
        [HttpPost]
        public  Result<Book> Create([FromBody] CreateBookVM createBookVM)
        {
            var book = Mapper.Map<CreateBookVM, Book>(createBookVM);
            return _bookService.Insert(book);
        }

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.AprovarLivro)]
        public Result<Book> Approve(string id) => _bookService.Approve(new Guid(id));

        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = _bookService.FreightOptions();
            return freightOptions;
        }

        [HttpGet("Top15NewBooks")]
        public IList<Book> Top15NewBooks() => _bookService.Top15NewBooks();

        [HttpGet("Random15Books")]
        public IList<Book> Random15Books() => _bookService.Random15Books();

        [Authorize("Bearer")]
        [HttpPost("Request/{id}")]
        public IActionResult RequestBook(string id)
        {         
            _bookUserService.Insert(new Guid(id));
            return Ok();
        }

        [Authorize("Bearer")]
        [HttpGet("ByTitle/{title}")]
        public IList<Book> ByTitle(string title) => _bookService.ByTitle(title);

        [Authorize("Bearer")]
        [HttpGet("ByAuthor/{author}")]
        public IList<Book> ByAuthor(string author) => _bookService.ByAuthor(author);

        [Authorize("Bearer")]
        [HttpGet("{page}/{items}")]
        public PagedList<Book> Paged(int page, int items)
        {
            return _bookService.GetAll(page, items);
        }

        [Authorize("Bearer")]
        [HttpGet()]
        public  PagedList<Book>GetAll()
        {
            return _bookService.GetAll(1, 15);
        }
    }
}
