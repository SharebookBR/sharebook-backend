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
    public class BookController : BaseDeleteController<Book, BaseViewModel>
    {
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _bookService;

        public BookController(IBookService bookService, IBookUserService bookUserService) : base(bookService)
        {
            _bookService = bookService;
            _bookUserService = bookUserService;
        }

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public Result<Book> Approve(string id) => _bookService.Approve(new Guid(id));

        [Authorize("Bearer")]
        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = _bookService.FreightOptions();
            return freightOptions;
        }

        [HttpGet("GranteeUsersByBookId/{bookId}")]
        public IList<User> GetGranteeUsersByBookId(string bookId) => _bookUserService.GetGranteeUsersByBookId(new Guid(bookId));

        [HttpGet("Slug/{slug}")]
        public Book Get(string slug) => _bookService.BySlug(slug);

        [HttpGet("Top15NewBooks")]
        public IList<Book> Top15NewBooks() => _bookService.Top15NewBooks();

        [HttpGet("Random15Books")]
        public IList<Book> Random15Books() => _bookService.Random15Books();

        [Authorize("Bearer")]
        [HttpGet("Title/{title}")]
        public IList<Book> ByTitle(string title) => _bookService.ByTitle(title);

        [Authorize("Bearer")]
        [HttpGet("Author/{author}")]
        public IList<Book> ByAuthor(string author) => _bookService.ByAuthor(author);

        [Authorize("Bearer")]
        [HttpPost("Request/{id}")]
        public IActionResult RequestBook(string id)
        {
            _bookUserService.Insert(new Guid(id));
            return Ok();
        }

        [Authorize("Bearer")]
        [HttpPost]
        public Result<Book> Create([FromBody] CreateBookVM createBookVM)
        {
            var book = Mapper.Map<Book>(createBookVM);
            return _service.Insert(book);
        }

        [Authorize("Bearer")]
        [HttpPut("{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public Result<Book> Update(Guid id, [FromBody] UpdateBookVM updateBookVM)
        {
            updateBookVM.Id = id;
            var book = Mapper.Map<Book>(updateBookVM);

            return _service.Update(book);
        }

        [Authorize("Bearer")]
        [HttpPut("{bookId}/User/{userId}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public IActionResult DonateBook(Guid bookId, Guid userId)
        {
            _bookUserService.DonateBook(bookId, userId);

            var jsonResult = "{\"message\":Livro doado com sucesso!\" }";
            return Ok(jsonResult);
        }
    }
}
