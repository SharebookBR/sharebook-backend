
using System;

namespace ShareBook.Service.Muambator
{
    public static class MuambatorConfigurator
    {
        public static string Token { get; private set; }

        public static bool IsActive { get; private set; }

        public static void Configure(string token, string isActive)
        {
            if ((string.IsNullOrEmpty(token) || string.IsNullOrEmpty(isActive)) || (isActive.ToLower() != "true" && isActive.ToLower() != "false"))
                return;

            Token = token;
            IsActive = Convert.ToBoolean(isActive.ToLower());
        }
    }
}
