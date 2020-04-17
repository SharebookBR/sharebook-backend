using Microsoft.AspNetCore.Mvc;
using Sharebook.Jobs;

namespace ShareBook.Api.Controllers.Generic
{
    public class BaseJobController : Controller
    {
        protected IJobExecutor _executor;
        protected string _validToken;

        protected bool _IsValidJobToken() => Request.Headers["Authorization"].ToString() == _validToken;
    }
}