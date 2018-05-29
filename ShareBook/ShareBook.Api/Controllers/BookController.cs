using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Service;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class BookController : BaseController<Book>
    {
        public BookController(IBookService bookService) : base(bookService)
        {
            SetDefault(x => x.Name);
        }
    }
}
