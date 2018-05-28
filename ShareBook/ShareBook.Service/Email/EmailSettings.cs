using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.Email
{
   public class EmailSettings
    {
        public string HostName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
    }
}
