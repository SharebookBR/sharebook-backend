using System;
using System.Collections.Generic;

namespace ShareBook.Service.CustomExceptions
{
    public class ShareBookException : Exception
    {
        public static Dictionary<Error, string> ErrorMessages = new Dictionary<Error, string>()
        {
            { Error.NotAuthorized, "Usuário não tem as permissões necessárias para efetuar esta ação." },
            { Error.NotFound, "Entidade não encontrada. Por favor, verifique." }
        };

        public enum Error
        {
            BadRequest = 400,
            NotAuthorized = 401,
            NotFound = 404         
        }

        public Error ErrorType { get; set; }

        public ShareBookException(Error error) : this(error, ErrorMessages[error]) { }
        public ShareBookException(Error error, string message) : base(message)
        {
            ErrorType = error;
        }
        public ShareBookException(string message) : base(message)
        {
            ErrorType = Error.BadRequest;
        }

    }
}
