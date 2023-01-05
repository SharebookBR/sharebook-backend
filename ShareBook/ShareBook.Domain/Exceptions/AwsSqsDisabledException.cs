using System;
using System.Runtime.Serialization;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class AwsSqsDisabledException : Exception
{
    public AwsSqsDisabledException(string message) : base(message)
    {
    }

    private AwsSqsDisabledException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

