using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Data.Entities.User
{
    public class UserMessage
    {
        public static class Validation
        {
            public const string Email = "O email é obrigatório";

            public const string Password = "A senha é obrigatória";
        }
    }
}
