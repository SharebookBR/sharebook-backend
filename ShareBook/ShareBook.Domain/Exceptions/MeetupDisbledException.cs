using System;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class MeetupDisbledException : Exception
{
    public MeetupDisbledException(string message) : base(message)
    {
    }
}

