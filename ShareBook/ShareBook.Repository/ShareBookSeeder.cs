using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Linq;

namespace ShareBook.Repository
{
    public  class ShareBookSeeder
    {

        private readonly ApplicationDbContext _context;

        // Teste123@
        private const string PASSWORD_HASH = "cWrRhnwyLmSOv3FIn7abuRevvV/GkGc1E/c66s02ujQ=";
        private const string PASSWORD_SALT = "xP+CoqfrCbbfIU9HPCd4rA==";


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
                    Linkedin = "linkedin.com/walter.cardoso",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now,
                    Address = new Address()
                    {
                        Street = "Rua teste",
                        Number = "1",
                        Complement = "apto 1",
                        Neighborhood = "Bairro teste",
                        PostalCode = "11111-111",
                        City = "São Paulo",
                        State = "SP",
                        Country = "Brasil"
                    }
                };

                var @operator = new User()
                {
                    Name = "Vagner",
                    Email = "vagner@sharebook.com",
                    Linkedin = "linkedin.com/vagner",
                    Profile = Profile.Administrator,
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now,
                    Address = new Address()
                    {
                        Street = "Rua teste",
                        Number = "2",
                        Complement = "apto 2",
                        Neighborhood = "Bairro teste",
                        PostalCode = "11111-111",
                        City = "São Paulo",
                        State = "SP",
                        Country = "Brasil"
                    }
                };

                var donor = new User()
                {
                    Name = "Rodrigo",
                    Email = "rodrigo@sharebook.com",
                    Linkedin = "linkedin.com/rodrigo",
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now,
                    Address = new Address()
                    {
                        Street = "Rua teste",
                        Number = "3",
                        Complement = "apto 3",
                        Neighborhood = "Bairro teste",
                        PostalCode = "11111-111",
                        City = "São Paulo",
                        State = "SP",
                        Country = "Brasil"
                    }
                };

                var facilitator = new User()
                {
                    Name = "Cussa",
                    Email = "cussa@sharebook.com",
                    Linkedin = "linkedin.com/cussa",
                    Profile = Profile.Administrator,
                    Password = PASSWORD_HASH,
                    PasswordSalt = PASSWORD_SALT,
                    CreationDate = DateTime.Now,
                    Address = new Address()
                    {
                        Street = "Rua teste",
                        Number = "4",
                        Complement = "apto 4",
                        Neighborhood = "Bairro teste",
                        PostalCode = "11111-111",
                        City = "São Paulo",
                        State = "SP",
                        Country = "Brasil"
                    }
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
                    Slug = "volta-ao-mundo-em-80-dias",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };


                var book2 = new Book()
                {
                    Author = "Robert Aley",
                    Title = "Teoria discursiva do direito",
                    FreightOption = FreightOption.State,
                    ImageSlug = "teoria-discursiva-do-direito.jpg",
                    Slug = "teoria-discursiva-do-direito",
                    User = donor,
                    Approved = true,
                    Category = dir,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book3 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The book of jonah",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-book-of-jonah.jpg",
                    Slug = "the-book-of-jonah",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book4 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The Hobbit",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-hobbit.jpg",
                    Slug = "the-hobbit",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book5 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "The Hobbit The And Back Again",
                    FreightOption = FreightOption.State,
                    ImageSlug = "the-hobbit-there-and-back-again.jpg",
                    Slug = "the-hobbit-there-and-back-again",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book6 = new Book()
                {
                    Author = "Zigurds",
                    Title = "Programando o Android",
                    FreightOption = FreightOption.State,
                    ImageSlug = "programando-o-android.jpg",
                    Slug = "programando-o-android",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-1),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book7 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "Senhor dos Aneis",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "senhor-dos-aneis.jpg",
                    Slug = "senhor-dos-aneis",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-5),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book8 = new Book()
                {
                    Author = "Esphyr",
                    Title = "Se Venden Gorras",
                    FreightOption = FreightOption.City,
                    ImageSlug = "se-venden-gorras.jpg",
                    Slug = "se-venden-gorras",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-6),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book9 = new Book()
                {
                    Author = "Adam",
                    Title = "Star Wars",
                    FreightOption = FreightOption.World,
                    ImageSlug = "star-wars.jpg",
                    Slug = "star-wars",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-8),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };


                var book10 = new Book()
                {
                    Author = "Brandon Rhodes",
                    Title = "Programação de Redes com Python",
                    FreightOption = FreightOption.State,
                    ImageSlug = "programacao-de-redes-com-python.jpg",
                    Slug = "programacao-de-redes-com-python",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-5),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book11 = new Book()
                {
                    Author = "Edgard",
                    Title = "Programação de jogo Android",
                    FreightOption = FreightOption.WithoutFreight,
                    ImageSlug = "programacao-de-jogo-android.jpg",
                    Slug = "programacao-de-jogo-android",
                    User = donor,
                    Approved = true,
                    Category = inf,
                    CreationDate = DateTime.Now.AddDays(-10),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book12 = new Book()
                {
                    Author = "Rick Riordan",
                    Title = "Percy Jackson e os Olimpianos",
                    FreightOption = FreightOption.State,
                    ImageSlug = "percy-jackson-e-os-olimpianos.jpg",
                    Slug = "percy-jackson-e-os-olimpianos",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book13 = new Book()
                {
                    Author = "Bendis",
                    Title = "Os Vingadores",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "os-vingadores.jpg",
                    Slug = "os-vingadores",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-9),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book14 = new Book()
                {
                    Author = "André Vianco",
                    Title = "Os Sete",
                    FreightOption = FreightOption.State,
                    ImageSlug = "os-sete.jpg",
                    Slug = "os-sete",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book16 = new Book()
                {
                    Author = "Shaxnom",
                    Title = "O Segredo das Sombras",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-segredo-das-sombras.jpg",
                    Slug = "o-segredo-das-sombras",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book17 = new Book()
                {
                    Author = "Jane Austen",
                    Title = "Orgulho e Preconceito ",
                    FreightOption = FreightOption.Country,
                    ImageSlug = "orgulho-e-preconceito.jpg",
                    Slug = "orgulho-e-preconceito",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-3),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book18 = new Book()
                {
                    Author = "J. R. R. Tolkien",
                    Title = "O Retorno do Rei",
                    FreightOption = FreightOption.WithoutFreight,
                    ImageSlug = "o-retorno-do-rei.png",
                    Slug = "o-retorno-do-rei",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-1),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book19 = new Book()
                {
                    Author = "Antoine",
                    Title = "O Pequeno Principe",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-pequeno-principe.jpg",
                    Slug = "o-pequeno-principe",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-3),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book20 = new Book()
                {
                    Author = "Aloisio Azevedo",
                    Title = "O cortiço",
                    FreightOption = FreightOption.State,
                    ImageSlug = "o-cortico.jpg",
                    Slug = "o-cortico",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-4),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book21 = new Book()
                {
                    Author = "Collen Hoover",
                    Title = "Nunca Jamais",
                    FreightOption = FreightOption.State,
                    ImageSlug = "nunca-jamais.jpg",
                    Slug = "nunca-jamais",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-5),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book22 = new Book()
                {
                    Author = "David Neves",
                    Title = "100 Segredos das Pessoas Felizes",
                    FreightOption = FreightOption.State,
                    ImageSlug = "100-segredos-das-pessoas-felizes.jpg",
                    Slug = "100-segredos-das-pessoas-felizes",
                    User = donor,
                    Approved = true,
                    Category = psico,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };


                var book23 = new Book()
                {
                    Author = "George R. R. Martin",
                    Title = "A Fúria dos Reis",
                    FreightOption = FreightOption.World,
                    ImageSlug = "a-furia-dos-reis.jpg",
                    Slug = "a-furia-dos-reis",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-4),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };

                var book15 = new Book()
                {
                    Author = "Maskus Suzak",
                    Title = "A Menina que Roubava Livros",
                    FreightOption = FreightOption.City,
                    ImageSlug = "a-menina-que-roubava-livros.jpg",
                    Slug = "a-menina-que-roubava-livros",
                    User = donor,
                    Approved = true,
                    Category = adv,
                    CreationDate = DateTime.Now.AddDays(-2),
                    ChooseDate = DateTime.Now.AddDays(5),
                    UserFacilitator = facilitator
                };


                var request = new BookUser()
                {
                    User = grantee,
                    Book = book5,
                    CreationDate = DateTime.Now,
                    Reason = "Quero muito esse livro.",
                    NickName = "Interessado 1"
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
