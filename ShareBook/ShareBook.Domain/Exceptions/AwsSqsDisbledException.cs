using System;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class AwsSqsDisbledException : Exception
{
    public AwsSqsDisbledException(string message) : base(message)
    {
    }
}

