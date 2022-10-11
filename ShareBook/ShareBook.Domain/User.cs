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
        public DateTime LastLogin { get; set; } = DateTime.Now;
        public string Linkedin { get; set; }
        public string Instagram { get; set; }
        public  string Phone{ get; set; }
        public Profile Profile { get;  set; } = Profile.User;
        public bool Active { get; set; } = true;
        public bool AllowSendingEmail { get; set; } = true;
        public virtual Address Address { get; set; }
        public virtual ICollection<BookUser> BookUsers { get; set; }
        public virtual ICollection<Book> BooksDonated { get; set; }
        public virtual ICollection<AccessHistory> Visitors { get; set; }

        public string ParentEmail { get; set; }
        public string ParentHashCodeAproval { get; set; }
        public bool ParentAproved { get; set; } = true;


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

        public void Change(string email, string name, string linkedin, string instagram, string phone, bool AllowSendingEmail)
        {
            this.Email = email;
            this.Name = name;
            this.Linkedin = linkedin;
            this.Instagram = instagram;
            this.Phone = phone;
            this.AllowSendingEmail = AllowSendingEmail;
        }

        public void ChangeAddress(Address address)
        {
            var AddressIdCopy = this.Address.Id;
            this.Address = address;

            this.Address.UserId = Id;
            this.Address.Id = AddressIdCopy;
        }

        public void ChangePassword(string password)
        {
            this.Password = password;
        }

        public bool IsBruteForceLogin()
        {
            var refDate = DateTime.Now.AddSeconds(-30);
            return LastLogin > refDate;
        }

        public string Location() => Address.City + "-" + Address.State;

        public int TotalBooksWon() => BookUsers.Where(b => b.Status == DonationStatus.Donated).ToList().Count ;

        public int TotalBooksDonated() => BooksDonated.Count(b => b.Status == BookStatus.WaitingSend || b.Status == BookStatus.Sent || b.Status == BookStatus.Received);

        // TODO: precisamos do lazy load pra esse método ser mais confiável.
        public bool HasAbandonedDonation(int maxLateDonationDays = 15)
        {
            if (BooksDonated == null) return false;
            return BooksDonated.Any(b => b.Status == BookStatus.AwaitingDonorDecision && (DateTime.Now - b.ChooseDate).Value.Days > maxLateDonationDays);
        }

        public void Anonymize()
        {
            Name = "USUÁRIO ANONIMIZADO";
            Email = "anonimizado_" + DateTime.Now.ToFileTime() + "@sharebook.com.br";
            Active = false;
            AllowSendingEmail = false;
            Linkedin = null;
            Instagram = null;
            Phone = null;
            ParentEmail = null;

            Address.City = null;
            Address.Complement = null;
            Address.Country = null;
            Address.Neighborhood = null;
            Address.Number = null;
            Address.PostalCode = null;
            Address.State = null;
            Address.Street = null;

        }
    }
}
