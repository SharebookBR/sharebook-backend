using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.Muambator
{
    public static class MuambatorConfigurator
    {
        public static string Token { get; private set; }

        public static void Configure(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return;
            }

            Token = token;
        }
    }
}
