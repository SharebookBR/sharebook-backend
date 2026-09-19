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

        [HttpGet("featured-printed-books")]
        public async Task<IActionResult> GetFeaturedPrintedBooksAsync()
        {
            var result = await _homeService.GetFeaturedPrintedBooksAsync();
            return Ok(result);
        }

        [HttpGet("categories-showcase")]
        public async Task<IActionResult> GetCategoriesShowcaseAsync()
        {
            var result = await _homeService.GetCategoriesShowcaseAsync();
            return Ok(result);
        }

        [HttpGet("top-downloaded-ebooks")]
        public async Task<IActionResult> GetTopDownloadedEbooksAsync([FromQuery] int days = 30)
        {
            var result = await _homeService.GetTopDownloadedEbooksAsync(days);
            return Ok(result);
        }
    }
}
