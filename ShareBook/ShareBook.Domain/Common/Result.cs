using FluentValidation.Results;
using System.Collections.Generic;
using System.Linq;

namespace ShareBook.Domain.Common;

public class Result : Result<object>
{
    public Result() : base(null) { }
    public Result(string SuccessMessage) : base(null)
    {
        this.SuccessMessage = SuccessMessage;
    }
}

public class Result<T>(ValidationResult? validationResult, T? value) where T : class
{
    public Result(T value) : this(null, value) { }
    public Result(ValidationResult? validationResult) : this(validationResult, null) { }

    public T? Value { get; set; } = value;
    public List<string> Messages { get; } = validationResult?.Errors.Select(x => x.ErrorMessage).ToList() ?? new List<string>();
    public string? SuccessMessage { get; set; }

    public bool Success { get { return Messages.Count == 0; } }
}
