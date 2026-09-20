using System;
using System.Runtime.Serialization;

namespace ShareBook.Domain.Exceptions;

[Serializable]
public class AwsSqsDisabledException(string message) : Exception(message)
{
}

