using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
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
        public Result<Book> Approve(string id) => ((IBookService)_service).Approve(new Guid(id));


        [HttpGet("FreightOptions")]
        public IList<dynamic> FreightOptions()
        {
            var freightOptions = ((IBookService)_service).GetAllFreightOptions();
            return freightOptions;
        }


        [Authorize("Bearer")]
        [HttpPost("RequestBook/{id}")]
        public IActionResult RequestBook(string id)
        {         
            _bookUserService.Insert(new Guid(id));
            return Ok();
        }
    }
}
