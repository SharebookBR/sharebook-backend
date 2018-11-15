using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Api.ViewModels
{
    public class ChangeUserPasswordByEmailAndHashCodeVM
    {
        public string Email { get; set; }

        public string HashCodePassword { get; set; }

        public string NewPassword { get; set; }
    }
}
