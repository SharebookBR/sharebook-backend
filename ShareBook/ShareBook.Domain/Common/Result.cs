﻿using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Domain.Common
{
    public class Result : Result<object>
    {
        public Result() : base(null) { }
        public Result(string SuccessMessage) : base(null)
        {
            this.SuccessMessage = SuccessMessage;
        }
    }

    public class Result<T> where T : class
    {
        public Result(T value) : this(null, value) { }
        public Result(ValidationResult validationResult) : this(validationResult, null) { }
        public Result(ValidationResult validationResult, T value)
        {
            Messages = validationResult?.Errors.Select(x => x.ErrorMessage).ToList() ?? new List<string>();
            Value = value;
        }

        public T Value { get; set; }
        public List<string> Messages { get; }
        public string SuccessMessage { get; set; }

        public bool Success { get { return Messages.Count == 0; } }
    }
}
