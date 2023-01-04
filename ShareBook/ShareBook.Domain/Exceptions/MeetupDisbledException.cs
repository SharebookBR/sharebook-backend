using System;

namespace ShareBook.Domain.Exceptions;

public class MeetupDisbledException : Exception
{
    public MeetupDisbledException(string message) : base(message)
    {
    }
}

