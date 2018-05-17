using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Data.Entities.User.In;
using ShareBook.Service;
using ShareBook.VM.Common;
using ShareBook.VM.User.In;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

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
        public async Task<ResultServiceVM> Post([FromBody]UserInVM  userInVM)
        {
            return await _userService.CreateUser(userInVM);
        }

        [HttpPost("Login")]
        public async Task<ResultServiceVM> Login([FromBody]UserInVM userInVM)
        {
            return await _userService.GetUserByEmailAndPasswordAsync(userInVM);
        }
    }
}
