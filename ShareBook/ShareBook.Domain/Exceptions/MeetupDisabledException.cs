using System;
using System.Runtime.Serialization;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class MeetupDisabledException : Exception
{
    public MeetupDisabledException(string message) : base(message)
    {
    }

    private MeetupDisabledException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

