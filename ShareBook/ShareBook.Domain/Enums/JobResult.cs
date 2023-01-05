using System;
namespace ShareBook.Domain.Enums
{
    public enum JobResult
    {
        Success,
        Error,
        AwsSqsDisabled,
        MeetupDisabled,
    }
}
