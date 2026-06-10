using Microsoft.AspNetCore.Mvc;
using ShareBook.Service.Home;
using System.Threading.Tasks;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        [HttpGet("categories-showcase")]
        public async Task<IActionResult> GetCategoriesShowcaseAsync()
        {
            var result = await _homeService.GetCategoriesShowcaseAsync();
            return Ok(result);
        }
    }
}
