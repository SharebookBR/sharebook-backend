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
        public ResultServiceVM Register([FromBody]UserVM userVM)
        {
            return _userService.Register(userVM);
        }

        [HttpPost("Login")]
        public UserVM Login([FromBody]UserVM userVM)
        {
            return _userService.Login(userVM);
        }
    }
}