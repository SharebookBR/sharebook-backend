using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShareBook.Domain.Common;
using System.Linq;

namespace ShareBook.Api.Filters
{
    public class ValidateModelStateFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values.Where(v => v.Errors.Count > 0)
                        .SelectMany(v => v.Errors)
                        .Select(v => v.ErrorMessage)
                        .ToList();

                var response = new Result();
                foreach (var error in errors)
                    response.Messages.Add(error);

                context.Result = new JsonResult(response)
                {
                    StatusCode = 400
                };
            }
        }
    }
}
