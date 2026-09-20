using System;
using System.Runtime.Serialization;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class MeetupDisabledException(string message) : Exception(message)
{
}

