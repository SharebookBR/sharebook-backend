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
using System.Linq.Expressions;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [GetClaimsFilter]
    [EnableCors("AllowAllHeaders")]
    public class BookController : Controller
    {
        private readonly IBookUserService _bookUserService;
        private readonly IBookService _service;
        private Expression<Func<Book, object>> _defaultOrder = x => x.Id;

        public BookController(IBookService bookService, IBookUserService bookUserService)
        {
            _service = bookService;
            _bookUserService = bookUserService;
        }

        protected void SetDefault(Expression<Func<Book, object>> defaultOrder)
        {
            _defaultOrder = defaultOrder;
        }

        [HttpGet()]
        [Authorize("Bearer")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public  PagedList<BooksVM> GetAll() => Paged(1, 15);

       [HttpGet("{page}/{items}")]
       [Authorize("Bearer")]
       [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public PagedList<BooksVM> Paged(int page, int items)
        {
            // corrigir para o parametro filter ser opcional
            var books = _service.Get<object>(x => x.Approved || !x.Approved, x => x.Title, page, items);
            var responseVM = Mapper.Map<List<BooksVM>>(books.Items);
            return new PagedList<BooksVM>()
            {
                Page = page,
                TotalItems = books.TotalItems,
                ItemsPerPage = items,
                Items = responseVM
            };
        }

        [HttpGet("{id}")]
        public Book GetById(string id) => _service.Get(new Guid(id));

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        [AuthorizationFilter(Permissions.Permission.ApproveBook)]
        public Result<Book> Approve(string id) => _service.Approve(new Guid(id));

        [Authorize("Bearer")]
        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = _service.FreightOptions();
            return freightOptions;
        }

        [Authorize("Bearer")]
        [HttpGet("GranteeUsersByBookId/{bookId}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public IList<User> GetGranteeUsersByBookId(string bookId) => _bookUserService.GetGranteeUsersByBookId(new Guid(bookId));

        [HttpGet("Slug/{slug}")]
        public Book Get(string slug) => _service.BySlug(slug);

        [HttpGet("Top15NewBooks")]
        public IList<Book> Top15NewBooks() => _service.Top15NewBooks();

        [HttpGet("Random15Books")]
        public IList<Book> Random15Books() => _service.Random15Books();

        [Authorize("Bearer")]
        [HttpGet("Title/{title}")]
        public IList<Book> ByTitle(string title) => _service.ByTitle(title);

        [Authorize("Bearer")]
        [HttpGet("Author/{author}")]
        public IList<Book> ByAuthor(string author) => _service.ByAuthor(author);

        [Authorize("Bearer")]
        [ProducesResponseType(typeof(Result), 200)]
        [HttpPost("Request/{id}")]
        public IActionResult RequestBook(string id)
        {
            _bookUserService.Insert(new Guid(id));
            var result = new Result
            {
                SuccessMessage = "Pedido realizo com sucesso!",
            };
            return Ok(result);
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
        [HttpPut("Donate/{bookId}")]
        [ProducesResponseType(typeof(Result), 200)]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public IActionResult DonateBook(Guid bookId, [FromBody] DonateBookUserVM donateBookUserVM)
        {
            _bookUserService.DonateBook(bookId, donateBookUserVM.UserId, donateBookUserVM.Note);

            var result = new Result
            {
                SuccessMessage = "Livro doado com sucesso!",
            };

            return Ok(result);
        }

        [Authorize("Bearer")]
        [HttpDelete("{id}")]
        [AuthorizationFilter(Permissions.Permission.DonateBook)]
        public Result Delete(Guid id) => _service.Delete(id);

        [Authorize("Bearer")]
        [HttpGet("Requested/{bookId}")]
        public Result Requested(Guid bookId)
        {
            var result = new Result
            {
                Value = new { bookRequested = _service.UserRequestedBook(bookId) },
            };

            return result;
        }
    }
}
