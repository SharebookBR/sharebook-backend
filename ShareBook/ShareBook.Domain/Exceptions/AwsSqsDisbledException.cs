using System;

namespace ShareBook.Domain.Exceptions;

public class AwsSqsDisbledException : Exception
{
    public AwsSqsDisbledException(string message) : base(message)
    {
    }
}

