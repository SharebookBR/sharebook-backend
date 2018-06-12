using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Service;
using System;
using Newtonsoft.Json;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : BaseController<Book>
    {
        public BookController(IBookService bookService) : base(bookService)
        {
            SetDefault(x => x.Title);
        }

        [Authorize("Bearer")]
        [HttpPost("Approve/{id}")]
        public Result<Book> Approve(string id) => ((IBookService)_service).Approve(new Guid(id));


        [HttpGet("FreightOptions")]
        public string FreightOptions()
        {
            var freightOptions = ((IBookService)_service).GetAllFreightOptions();
            return JsonConvert.SerializeObject(freightOptions);
        }
    }
}
