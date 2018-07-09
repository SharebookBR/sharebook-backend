using Microsoft.AspNetCore.Mvc;

namespace ShareBook.Api.Controllers
{
    public class ClientSpaController : Controller
    {
        public IActionResult Index()
        {
            return File("~/index.html", "text/html");
        }
    }
}