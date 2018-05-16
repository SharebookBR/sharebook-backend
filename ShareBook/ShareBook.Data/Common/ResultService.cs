using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Data.Common
{
    public class ResultService
    {
        public ResultService(ValidationResult fluentResult)
        {
            this.Messages = fluentResult.Errors.Select(x => x.ErrorMessage).ToList();
            this.SuccessMessage = null;
        }

        public ResultService()
        {
            this.Messages = new List<string>();
            this.SuccessMessage = null;
        }

        public bool Success
        {
            get
            {
                return this.Messages.Count == 0;
            }
        }

        public List<string> Messages { get; set; }

        public string SuccessMessage { get; set; }

        public void AddMessages(ResultService resultService)
        {
            this.Messages.AddRange(resultService.Messages);
        }
    }
}
