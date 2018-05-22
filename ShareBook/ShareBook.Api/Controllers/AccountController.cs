using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Service;
using ShareBook.VM.Common;
using ShareBook.VM.User.Model;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<ResultServiceVM> Post([FromBody]UserVM userVM)
        {
            return await _userService.CreateUserAsync(userVM);
        }

        [HttpPost("Login")]
        public async Task<UserVM> Login([FromBody]UserVM userVM)
        {
            return await _userService.GetUserByEmailAndPasswordAsync(userVM);
        }
    }
}