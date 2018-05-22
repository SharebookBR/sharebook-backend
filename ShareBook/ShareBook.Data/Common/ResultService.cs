using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace ShareBook.Data.Common
{
    public class ResultService
    {
        public ResultService(ValidationResult fluentResult)
        {
            Messages = fluentResult.Errors.Select(x => x.ErrorMessage).ToList();
        }

        public List<string> Messages { get; set; }

        public bool Success => Messages.Count == 0;

        public string SuccessMessage { get; set; }

        public void AddMessages(ResultService resultService)
        {
            Messages.AddRange(resultService.Messages);
        }
    }
}