using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class ChangePasswordUserVM
    {
        public string Email { get; set; }

        public string NewPassword { get; set; }

        public string OldPassword { get; set; }
    }
}
