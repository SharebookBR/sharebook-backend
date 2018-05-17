using System;

namespace ShareBook.Data.Entities.User
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public User()
        {
           
        }

        public User(Guid id, string email, string password)
        {
            this.Id = id;
            this.Email = email.ToLower();
            this.Password = password;
        }


    }
}
