using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Linq;

namespace ShareBook.Repository
{
    public class ShareBookSeeder
    {

        private readonly ApplicationDbContext _context;

        // 123456
        private const string PASSWORD_HASH = "n71pJuPLLg4EJkRBf+SRDXHD3x5f1sNI+3Fi5bSjdx4=";
        private const string PASSWORD_SALT = "Uo5G5EKyKh5GnXy0D57i0w==";


        public ShareBookSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            _context.Database.EnsureCreated();



            if (!(_context.Users.Any()
                && _context.Books.Any()
                && _context.Categories.Any()
                && _context.Meetups.Any()))
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

                var @raffa = new User()
                {
                    Name = "Raffa dev",
                    Email = "raffacabofrio@gmail.com",
                    Linkedin = "linkedin.com/raffacabofrio",
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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
                    Status = BookStatus.Available,
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

                var meetup1 = new Meetup()
                {
                    SymplaEventId = 1741126,
                    Title = "Mensageria na Azure - do conceito à prática.",
                    Description = "<div>Nesta live convidei Vinícius Climaco que estará explicando a importância e o motivo pelo qual temos tanta utilização da Mensageria e não paramos no conceito! Teremos muito hand´s on através de demos com Azure Service Bus e DotNet 6.</div><div>Vem com a gente em mais uma stack, contamos com sua participação!</div><div><br></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\"   rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-10-19T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/mensageria-na-azure---do-conceito-a-pratica.png",
                    YoutubeUrl = null,
                    SymplaEventUrl = "https://www.sympla.com.br/mensageria-na-azure---do-conceito-a-pratica__1743065",
                };

                var meetup2 = new Meetup()
                {
                    SymplaEventId = 1741126,
                    Title = "Qualidade de vida",
                    Description = "<div>Todo profissional que inicia a construção de sua carreira sabe que um dia poderá se assumir uma função de liderança. Para quem está na liderança é importante entender o processo da equipe e os indicadores de qualidade de vida.</div><div><br></div><div>Como está a sua qualidade de vida e do seu time? Vale muito a pensar a respeito, olhar para os próprios hábitos e ajustar o que for preciso.&nbsp;</div><div><br></div><div>Mas o que está por trás de uma vida com mais qualidade? Conquistar o emprego perfeito. Realizar os seus sonhos. Ter saúde. Amar e ser amado pelos familiares e amigos. Ser feliz, enfim. As possibilidades são muitas e esse é o tema do nosso meetup! =)</div><div><br></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-10-06T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/qualidade-de-vida.png",
                    YoutubeUrl = "https://youtube.com/watch?v=6LUqY0yl7YQ",
                    SymplaEventUrl = "https://www.sympla.com.br/qualidade-de-vida__1741126"
                };

                var meetup3 = new Meetup()
                {
                    SymplaEventId = 1738605,
                    Title = "Azure Data Factory - parte 2 de 2",
                    Description = "<div>ETL. Extrair, transformar e carregar. Coletar dados de várias fontes e reuni-los para dar suporte à descoberta, à geração de relatórios, à análise e à tomada de decisões.</div><div><br></div><div>Azure Data Factory é um ETL Serverless totalmente gerenciado. Integre visualmente as fontes de dados usando mais de 90 conectores internos livres de manutenção sem custo adicional. Construa processos ETL e ELT sem código com facilidade em um ambiente visual intuitivo ou escreva seu próprio código.</div><div><br></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-10-01T13:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/azure-data-factory---parte-2-de-2.png",
                    YoutubeUrl = "https://youtube.com/watch?v=_AmxQNuaxsY",
                    SymplaEventUrl = "https://www.sympla.com.br/azure-data-factory---parte-2-de-2__1738605"
                };

                var meetup4 = new Meetup()
                {
                    SymplaEventId = 1724107,
                    Title = "Team Building",
                    Description = "<div>Qualquer um forma um grupo. Alguns formam times. Mas <b>apenas os melhores formam times de alta performance</b>. Nesse meetup vamos discutir sobre o team building.&nbsp;</div><div><br></div><div>- renovar a motivação.</div><div>- aproximar os colaboradores.</div><div>- identificação de forças e fraquezas.</div><div>- potencializar o talento.</div><div><b><br></b></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-09-21T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/team-building.png",
                    YoutubeUrl = "https://youtube.com/watch?v=UKonq6Yff5c",
                    SymplaEventUrl = "https://www.sympla.com.br/team-building__1724107"
                };

                var meetup5 = new Meetup()
                {
                    SymplaEventId = 1717056,
                    Title = "Fracassos e aprendizados na carreira",
                    Description = "<div>Fracassar, uma palavra temida por qualquer pessoa. Se admitir um erro é difícil, aceitar um fracasso é ainda pior.&nbsp;<span style=\"letter-spacing: 0px;\">O receio de não atingir o objetivo desejado também está no fato de que ninguém inicia algo com a intenção de dar errado. Mesmo que o indivíduo se dedique ao máximo em cada passo, sempre há obstáculos e tropeços na trajetória que podem levar ao fracasso. Perder faz parte da vitória, do crescimento e do processo evolutivo de cada ser humano. Sem isso, não haveria motivos pelos quais perseverar, lutar e continuar acreditando em nossa força.</span></div><div><span style=\"letter-spacing: 0px;\"><br></span></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-09-14T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/fracassos-e-aprendizados-na-carreira.png",
                    YoutubeUrl = "https://youtube.com/watch?v=5bWHRYnTEws",
                    SymplaEventUrl = "https://www.sympla.com.br/fracassos-e-aprendizados-na-carreira__1717056"
                };

                var meetup6 = new Meetup()
                {
                    SymplaEventId = 1711308,
                    Title = "GitHub Actions",
                    Description = "<div>GitHub Actions é uma plataforma de integração contínua e entrega contínua (CI/CD) que permite automatizar a sua compilação, teste e pipeline de implantação.&nbsp;Você pode criar fluxos de trabalho que constroem e testam cada pull request para seu repositório ou implementam pull requests mesclados para produção.</div><div><br></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-09-09T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/github-actions.png",
                    YoutubeUrl = "https://youtube.com/watch?v=ygjyH6tL7Nk",
                    SymplaEventUrl = "https://www.sympla.com.br/github-actions__1711308"
                };

                var meetup7 = new Meetup()
                {
                    SymplaEventId = 1709335,
                    Title = "Desenvolvimento de software sustentável",
                    Description = "<div><div>Sustentabilidade é a capacidade de sustentação ou conservação de um processo ou sistema.&nbsp;<span style=\"letter-spacing: 0px;\">A palavra sustentável deriva do latim sustentare e significa sustentar, apoiar, conservar e cuidar.</span></div></div><div><br></div><div>Construir software sustentável, e fazer isso de maneira escalável, exige a criação de um ecossistema confiável de pessoas, padrões, ferramentas e boas práticas.</div><div><br></div><div><b>Nosso evento será transmitido ao vivo via YouTube:&nbsp;</b><br><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\"  rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></div>",
                    StartDate = DateTime.Parse("2022-09-07T19:00:00"),
                    Cover = "https://www.sharebook.com.br/Images/Meetup/desenvolvimento-de-software-sustentavel.png",
                    YoutubeUrl = null,
                    SymplaEventUrl = "https://www.sympla.com.br/desenvolvimento-de-software-sustentavel__1709335",
                };

                var meetup8 = new Meetup()
                {
                    SymplaEventId = 1706178,
                    Title = "Java Spring Boot - Criando uma API CRUD do zero - 2 de 2",
                    Description = "<p>Essa é parte final do nosso vídeo com muita mão na massa. Bora criar nossa API CRUD com Spring Boot!</p><p><b>Nosso evento será transmitido ao vivo via YouTube: </b></p><p><a href=\"https://www.youtube.com/c/sharebookBR\" target=\"_blank\" rel=\"nofollow noopener noreferrer\">https://www.youtube.com/c/sharebookBR</a><br></p>",
                    StartDate = DateTime.Parse("2022-09-03T13:00:00"),
                    Cover = "https://sharebookbr.github.io/sharebook-cursos/covers/java-springboot-parte2.png",
                    YoutubeUrl = null,
                    SymplaEventUrl = "https://www.sympla.com.br/java-spring-boot---criando-uma-api-crud-do-zero---2-de-2__1706178"
                };

                var meetup9 = new Meetup()
                {
                    SymplaEventId = 2432671,
                    Title = "GITHUB ACTIONS NA PRÁTICA",
                    Description = "<div>O GitHub Actions é uma ferramenta poderosa para automação de fluxos de trabalho de desenvolvimento. Ao permitir que você defina e execute uma série de ações baseadas em eventos específicos do GitHub, como push, pull request ou issues, ele simplifica tarefas complexas, desde a integração contínua até a implantação contínua. Com sua flexibilidade e capacidade de personalização, o GitHub Actions capacita os desenvolvedores a otimizarem seus processos de desenvolvimento, melhorando a qualidade do código, reduzindo erros e aumentando a eficiência geral do projeto. Em um mundo onde cada segundo conta, o GitHub Actions se torna um aliado indispensável na busca pela excelência técnica.</div><div><br></div><div><b>Palestrante</b>: Renan Barbosa</div><div><b>Host</b>: Raffaello Damgaard</div><div><br></div><div><b>Nossa live será transmitida ao vivo via You Tube</b>:</div><div><a href=\"https://www.youtube.com/watch?v=2d7PchqkRVQ\" target=\"_blank\"  rel=\"nofollow noopener noreferrer\">https://www.youtube.com/watch?v=2d7PchqkRVQ</a><br></div>",
                    StartDate = DateTime.Now.AddDays(7), // Upcoming = true
                    Cover = "https://www.sharebook.com.br/Images/Meetup/github-actions-na-pratica.png",
                    YoutubeUrl = "https://www.sharebook.com.br/Images/Meetup/github-actions-na-pratica.png",
                    SymplaEventUrl = "https://www.sympla.com.br/java-spring-boot---criando-uma-api-crud-do-zero---2-de-2__1706178"
                };

                _context.Categories.AddRange(adm, dir, psico, med, eng, geo_his, cien, art);
                _context.Users.AddRange(grantee, @operator, raffa);
                _context.Books.AddRange(book1, book2, book3, book4, book5, book6, book7,
                    book8, book9, book10, book11, book12, book13, book14, book15, book16,
                    book16, book18, book19, book20, book21, book22, book23);

                _context.BookUser.Add(request);
                _context.Meetups.AddRange(meetup1, meetup2, meetup3, meetup4, meetup5, meetup6, meetup7, meetup8, meetup9);

                _context.SaveChanges();
            }

        }

    }
}
