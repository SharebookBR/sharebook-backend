using ShareBook.Domain.Common;
using ShareBook.Domain.Enums;
using ShareBook.Helper.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ShareBook.Domain
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
        public string HashCodePassword { get; set; }
        public DateTime HashCodePasswordExpiryDate { get; set; }
        public string Linkedin { get; set; }
        public  string Phone{ get; set; }
        public Profile Profile { get;  set; } = Profile.User;
        public bool Active { get; set; } = true;
        public bool AllowSendingEmail { get; set; } = true;
        public virtual Address Address { get; set; }
        public virtual ICollection<BookUser> BookUsers { get; set; }
        public virtual ICollection<Book> BooksDonated { get; set; }

        public bool PasswordIsStrong()
        {
            Regex rgx = new Regex(@"(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])[A-Za-z0-9\d$@$!%_*_?&#.,-_:;]{8,}");
            if (string.IsNullOrEmpty(Password) || !rgx.IsMatch(Password)) return false;

            return true;
        }

        public User Cleanup()
        {
            this.Password = string.Empty;
            this.PasswordSalt = string.Empty;
            return this;
        }

        public void GenerateHashCodePassword()
        {
            this.HashCodePassword =  Guid.NewGuid().ToString();
            this.HashCodePasswordExpiryDate = DateTime.Now.AddDays(1); 
        }

        public bool HashCodePasswordIsValid(string hashCodePassword)
             => hashCodePassword == this.HashCodePassword 
                && (this.HashCodePasswordExpiryDate.Date == DateTime.Now.AddDays(1).Date
                   || this.HashCodePasswordExpiryDate.Date == DateTime.Now.Date);

        public void Change(string email, string name, string linkedin, string phone)
        {
            this.Email = email;
            this.Name = name;
            this.Linkedin = linkedin;
            this.Phone = phone;
        }

        public void ChangeAddress(Address address)
        {
            this.Address = address;
            this.Address.UserId = Id;
        }

        public void ChangePassword(string password)
        {
            this.Password = password;
        }

        public string Location() => Address.City + "-" + Address.State;

        public int TotalBooksWon() => BookUsers.Where(b => b.Status == DonationStatus.Donated).ToList().Count ;

        public int TotalBooksDonated() => BooksDonated.Count;
    }
}
