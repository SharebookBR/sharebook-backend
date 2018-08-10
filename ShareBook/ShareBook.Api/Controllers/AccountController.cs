using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.Filters;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Infra.CrossCutting.Identity;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;
using System;
using System.Net.Http;
using System.Threading;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    [GetClaimsFilter]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IApplicationSignInManager _signManager;

       

        public AccountController(IUserService userService, IApplicationSignInManager signManager)
        {
            _userService = userService;
            _signManager = signManager;
        }

        #region GET
        [Authorize("Bearer")]
        [HttpGet]
        public User Get()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return _userService.Get(id);
        }


        [Authorize("Bearer")]
        [HttpGet("Profile")]
        public object Profile()
        {
            var id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            return new { profile = _userService.Get(id).Profile.ToString() };
        }
        #endregion


        #region POST
        [HttpPost("Register")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(409)]
        public IActionResult Post([FromBody]RegisterUserVM registerUserVM,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<RegisterUserVM, User>(registerUserVM);

            var result = _userService.Insert(user);

            if (result.Success)
                return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));

            return Conflict(result);
        }

        [HttpPost("Login")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(404)]
        public IActionResult Login([FromBody]LoginUserVM loginUserVM,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<LoginUserVM, User>(loginUserVM);

            var result = _userService.AuthenticationByEmailAndPassword(user);

            if (result.Success)
            {
                var response = new Result
                {
                    Value = _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations)
                };

                return Ok(response);
            }

            return NotFound(result);
        }
        #endregion

        #region PUT
        [Authorize("Bearer")]
        [HttpPut]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(409)]
        public IActionResult Update([FromBody]UpdateUserVM updateUserVM,
           [FromServices]SigningConfigurations signingConfigurations,
           [FromServices]TokenConfigurations tokenConfigurations)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = Mapper.Map<UpdateUserVM, User>(updateUserVM);

            var result = _userService.Update(user);

            if (result.Success)
                return Ok(_signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations));

            return Conflict(result);
        }


        [Authorize("Bearer")]
        [HttpPut("ChangePassword")]
        public Result<User> ChangePassword([FromBody]ChangePasswordUserVM changePasswordUserVM)
        {
            var user = new User() { Password = changePasswordUserVM.OldPassword };

            return _userService.ChangeUserPassword(user, changePasswordUserVM.NewPassword);
        }
        #endregion
    }
}
