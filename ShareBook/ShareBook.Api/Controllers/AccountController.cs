using Microsoft.AspNetCore.Mvc;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;
using ShareBook.Service;

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
        public Result<User> Post([FromBody]User user) => _userService.Insert(user);

        [HttpPost("Login")]
        public object Login([FromBody]User user,
            [FromServices]SigningConfigurations signingConfigurations)
        {
            user = _userService.GetByEmailAndPassword(user);
            ApplicationSignInManager signManager = new ApplicationSignInManager();
            var obj = signManager.GenerateToken(user, signingConfigurations);
            return obj;
        }
    }
}
