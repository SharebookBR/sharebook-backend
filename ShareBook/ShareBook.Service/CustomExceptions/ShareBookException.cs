using System;

namespace ShareBook.Service.CustomExceptions
{
    public class ShareBookException : Exception
    {
        public enum Error
        {
            NotAuthorized = 401,
            NotFound = 404
        }

        public Error ErrorType { get; set; }

        public ShareBookException(Error error) : this(error, null) { }
        public ShareBookException(Error error, string message) : base(message) { }
    }
}
