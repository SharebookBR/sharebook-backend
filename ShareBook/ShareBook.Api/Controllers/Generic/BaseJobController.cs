using Microsoft.AspNetCore.Mvc;
using Sharebook.Jobs;

namespace ShareBook.Api.Controllers.Generic
{
    public class BaseJobController: Controller
    {
        protected IJobExecutor _executor;
        protected string _validToken;

        protected bool _IsValidJobToken()
        {
            var userToken = Request.Headers["Authorization"].ToString();

            if (userToken == _validToken) return true;

            return false;
        }
    }

    
}
