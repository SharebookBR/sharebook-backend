using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Linq;

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

            if ( !(_context.Users.Any() 
                && _context.Books.Any()
                && _context.Categories.Any()))
            {
                var grantee = new User()
                {
                    Name = "Walter Vinicius Lopes Cardoso",
                    Email = "walter@sharebook.com",
                    PostalCode = "04473-190",
                    Linkedin = "linkedin.com/walter.cardoso",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now
                };

                var @operator = new User()
                {
                    Name = "Vagner",
                    Email = "vagner@sharebook.com",
                    PostalCode = "04516-190",
                    Linkedin = "linkedin.com/vagner",
                    Profile = Profile.Administrator,
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now
                };

                var donor = new User()
                {
                    Name = "Rodrigo",
                    Email = "rodrigo@sharebook.com",
                    PostalCode = "017672-100",
                    Linkedin = "linkedin.com/rodrigo",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now
                };

                var dir = new Category() { Name = "Direito", CreationDate = DateTime.Now };
                var psico = new Category() { Name = "Psicologia", CreationDate = DateTime.Now };
                var adm = new Category() { Name = "Administração", CreationDate = DateTime.Now };
                var adv = new Category() { Name = "Aventura" };
                var eng = new Category() { Name = "Engenharia", CreationDate = DateTime.Now };
                var cien = new Category() { Name = "Ciências Biógicas", CreationDate = DateTime.Now };
                var geo_his = new Category() { Name = "Geografia e História", CreationDate = DateTime.Now };
                var art = new Category() { Name = "Artes", CreationDate = DateTime.Now };
                var med = new Category() { Name = "Medicina", CreationDate = DateTime.Now };
                var eco = new Category() { Name = "Economia", CreationDate = DateTime.Now };
                var inf = new Category() { Name = "Informática", CreationDate = DateTime.Now };

                var book1 = new Book()
                {
                    Author = "Julio Verne",
                    Title = "Volta ao mundo em 80 dias",
                    FreightOption = FreightOption.World,
                    ImageSlug = "volta-ao-mundo-em-80-dias.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1)
                };


                var book2 = new Book()
                {
                    Author = "Robert Aley",
                    Title = "Teoria discursiva do direito",
                    FreightOption = FreightOption.State,
                    ImageSlug = "teoria-discursiva-do-direito.jpg",
                    User = donor,
                    Approved = true,
                    Category = dir,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book3 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The book of jonah",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-book-of-jonah.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book4 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The Hobbit",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-hobbit.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book5 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The Hobbit The And Back Again",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-hobbit-there-and-back-again.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book6 = new Book()
                {
                    Author = "Zigurds",
                    Title = "Programando o Android",
                    FreightOption = FreightOption.State,
                    ImageSlug = "programando-o-android.jpg",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-1)
                };

                var book7 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "Senhor dos Aneis",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "senhor-dos-aneis.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-5)
                };

                var book8 = new Book()
                {
                    Author = "Esphyr",
                    Title = "Se Venden Gorras",
                    FreightOption = FreightOption.City,
                    ImageSlug = "se-venden-gorras.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-6)
                };

                var book9 = new Book()
                {
                    Author = "Adam",
                    Title = "Star Wars",
                    FreightOption = FreightOption.World,
                    ImageSlug = "star-wars.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-8)
                };


                var book10 = new Book()
                {
                    Author = "Brandon Rhodes",
                    Title = "Programação de Redes com Python",
                    FreightOption = FreightOption.State,
                    ImageSlug = "programacao-de-redes-com-python.jpg",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-5)
                };

                var book11 = new Book()
                {
                    Author = "Edgard",
                    Title = "Programação de jogo Android",
                    FreightOption = FreightOption.WithoutFreight,
                    ImageSlug = "programacao-de-jogo-android.jpg",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-10)
                };

                var book12 = new Book()
                {
                    Author = "Rick Riordan",
                    Title = "Percy Jackson e os Olimpianos",
                    FreightOption = FreightOption.State,
                    ImageSlug = "percy-jackson-e-os-olimpianos.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book13 = new Book()
                {
                    Author = "Bendis",
                    Title = "Os Vingadores",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "os-vingadores.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-9)
                };

                var book14 = new Book()
                {
                    Author = "André Vianco",
                    Title = "Os Sete",
                    FreightOption = FreightOption.State,
                    ImageSlug = "os-sete.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1)
                };

                var book16 = new Book()
                {
                    Author = "Shaxnom",
                    Title = "O Segredo das Sombras",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-segredo-das-sombras.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };

                var book17 = new Book()
                {
                    Author = "Jane Austen",
                    Title = "Orgulho e Preconceito ",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "orgulho-e-preconceito.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-3)
                };

                var book18 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "O Retorno do Rei",
                    FreightOption = FreightOption.WithoutFreight,
                    ImageSlug = "o-retorno-do-rei.png",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1)
                };

                var book19 = new Book()
                {
                    Author = "Antoine",
                    Title = "O Pequeno Principe",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-pequeno-principe.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-3)
                };

                var book20 = new Book()
                {
                    Author = "Aloisio Azevedo",
                    Title = "O cortiço",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-cortico.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-4)
                };

                var book21 = new Book()
                {
                    Author = "Collen Hoover",
                    Title = "Nunca Jamais",
                    FreightOption = FreightOption.State,
                    ImageSlug = "nunca-jamais.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-5)
                };

                var book22 = new Book()
                {
                    Author = "David Neves",
                    Title = "100 Segredos das Pessoas Felizes",
                    FreightOption = FreightOption.State,
                    ImageSlug = "100-segredos-das-pessoas-felizes.jpg",
                    User = donor,
                    Approved = true,
                    Category = psico,
                    CreationDate = DateTime.Now.AddDays(-2)
                };


                var book23 = new Book()
                {
                    Author = "George R. R. Martin",
                    Title = "A Fúria dos Reis",
                    FreightOption = FreightOption.World,
                    ImageSlug = "a-furia-dos-reis.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-4)
                };

                var book15 = new Book()
                {
                    Author = "Maskus Suzak",
                    Title = "A Menina que Roubava Livros",
                    FreightOption = FreightOption.City,
                    ImageSlug = "a-menina-que-roubava-livros.jpg",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2)
                };


                var request = new BookUser()
                {
                    User = grantee,
                    Book = book5,
                    CreationDate = DateTime.Now
                };

                _context.Categories.AddRange(adm, dir, psico, med, eng, geo_his, cien, art);
                _context.Users.AddRange(grantee, @operator);
                _context.Books.AddRange(book1, book2, book3, book4, book5, book6, book7,
                    book8, book9, book10, book11, book12, book13, book14, book15, book16,
                    book16, book18, book19, book20 , book21 , book22 , book23);

                _context.BookUser.Add(request);
                _context.SaveChanges();
            }

        }
        
    }
}
