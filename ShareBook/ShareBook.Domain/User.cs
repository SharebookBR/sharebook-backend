using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ShareBook.Domain
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string Linkedin { get; set; }
        public string PostalCode { get; set; }
        public  string Phone{ get; set; }
        public Profile Profile { get;  set; } = Profile.User;
        public virtual ICollection<BookUser> BookUsers { get; set; }

        public bool PasswordIsStrong()
        {
            Regex rgx = new Regex(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[$@$!%*?&.,])[A-Za-z\d$@$!%*?&].{8,}");
            if (string.IsNullOrEmpty(Password) || !rgx.IsMatch(Password)) return false;

            return true;
        }

        public void ChangeEmail(string email)
        {
            this.Email = email;
        }

        public void ChangeName(string name)
        {
            this.Name = name;
        }
        
        public void ChangeLinkedin(string linkedin)
        {
            this.Linkedin = linkedin;
        }

        public void ChangePostalCode(string postalCode)
        {
            this.PostalCode = postalCode;
        }

        public void ChangePhone(string phone)
        {
            this.Phone = phone;
        }

        public void ChangePassword(string password)
        {
            this.Password = password;
        }
    }
}
