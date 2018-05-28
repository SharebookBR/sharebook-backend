using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;

namespace ShareBook.Api.Controllers
{
    [GetClaimsFilter]
    public class BaseController : Controller
    {
    }
}
