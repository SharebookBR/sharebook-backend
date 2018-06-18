using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareBook.Repository
{
    public  class ShareBookSeeder
    {

        private readonly ApplicationDbContext _context;

        private const string PASSWORD_HASH = "9XurTqQsYQY1rtAGXRfwEWO/ROghN3DFx9lTT75i/0s=";
        private const string PASSWORD_SALT = "1x7XxoaSO5I0QGIdARCh5A==";

        public ShareBookSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            _context.Database.EnsureCreated();

            if ( !(_context.Users.Any() && _context.Books.Any()))
            {
                var grantee = new User()
                {
                    Name = "Walter Vinicius Lopes Cardoso",
                    Email = "walter@sharebook.com",
                    Cep = "04473-190",
                    Linkedin = "linkedin.com/walter.cardoso",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT
                };

                var @operator = new User()
                {
                    Name = "Vagner",
                    Email = "vagner@sharebook.com",
                    Cep = "04516-190",
                    Linkedin = "linkedin.com/vagner",
                    Profile = Profile.Administrator,
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT
                };

                var donor = new User()
                {
                    Name = "Rodrigo",
                    Email = "rodrigo@sharebook.com",
                    Cep = "017672-100",
                    Linkedin = "linkedin.com/rodrigo",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT
                };

                var lordTheRings = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The Lord of the Rings: The Fellowship of the Ring",
                    FreightOption = FreightOption.World,
                    Image = "Fellowship.jpeg",
                    User = donor
                };


                var got = new Book()
                {
                    Author = "George R.R. Martin",
                    Title = "Game of Thrones",
                    FreightOption = FreightOption.State,
                    Image = "got.jpeg",
                    User = donor
                };

                var cleanCode = new Book()
                {
                    Author = "Robert Cecil Martin",
                    Title = "Clean Code",
                    FreightOption = FreightOption.State,
                    Image = "cleancode.jpeg",
                    User = donor
                };

                var agile = new Book()
                {
                    Author = "Agile Principles, Patterns, and Practices in C#",
                    Title = "Robert Cecil Martin",
                    FreightOption = FreightOption.WithoutFreight,
                    Image = "agile.jpeg",
                    User = donor
                };


                var ddd = new Book()
                {
                    Author = "Domain Driven Design",
                    Title = "Eric Evans",
                    FreightOption = FreightOption.WithoutFreight,
                    Image = "ddd.jpeg",
                    User = donor
                };

                var hp1 = new Book()
                {
                    Author = "Harry Potter and the Philosopher's Stone",
                    Title = "J. K. Rowling",
                    FreightOption = FreightOption.Country,
                    Image = "hp1.jpeg",
                    User = donor
                };


                var investimentos = new Book()
                {
                    Author = "Investimentos Inteligentes",
                    Title = "Gustavo Cerbasi",
                    FreightOption = FreightOption.City,
                    Image = "investimentos.jpeg",
                    User = donor
                };

                _context.Users.AddRange(grantee, @operator);
                _context.Books.AddRange(agile, cleanCode, got, lordTheRings, ddd, investimentos, hp1);
                _context.SaveChanges();
            }

        }
        
    }
}
