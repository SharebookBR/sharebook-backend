using ShareBook.Domain;
using ShareBook.Service;
using System;
using System.Text;
using Xunit;

namespace ShareBook.Test.Unit.Services
{
    public class EmailTemplateTests
    {
        readonly IEmailTemplate emailTemplate;

        private User user;
        private Book book;
        private User administrator;
        private User requestingUser;

        public EmailTemplateTests()
        {
            emailTemplate = new EmailTemplate();

             user = new User()
            {
                Id = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
                Name = "Rodrigo",
                Email = "rodrigo@sharebook.com",
                Linkedin = "linkedin.com/rodrigo",
                Profile = Domain.Enums.Profile.User
            };

             book = new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                ImageSlug = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                User = user
            };

            requestingUser = new User()
            {

                Id = new Guid("5489A967-9320-4350-FFFF-08D5CC8498F3"),
                Name = "Walter Vinicius",
                Email = "walter@sharebook.com",
                Linkedin = "linkedin.com/walter",
                Profile = Domain.Enums.Profile.User
            };

            administrator = new User()
            {
                Id = new Guid("5489A967-AAAA-BBBB-CCCC-08D5CC8498F3"),
                Name = "Cussa Mitre",
                Email = "cussa@sharebook.com",
                Profile = Domain.Enums.Profile.Administrator
            };
        }

        [Fact]
        public void VerifyEmailNewBookInsertedParse()
        {
            var vm = new
            {
                Book = book,
                Administrator = administrator
            };

            var result = emailTemplate.GenerateHtmlFromTemplateAsync("NewBookInsertedTemplate", vm).Result;
            //<!DOCTYPE html>\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <title>Novo livro cadastrado - Sharebook</title>\r\n</head>\r\n<body>\r\n    <p>\r\n        Olá Cussa Mitre,\r\n    </p>\r\n    <p>\r\n        Um novo livro foi cadastrado. Veja mais informações abaixo:\r\n    </p>\r\n\r\n    <ul>\r\n        <li><strong>Livro: </strong>Lord of the Rings</li>\r\n        <li><strong>Autor: </strong>J. R. R. Tolkien</li>\r\n        <li><strong>Usuário: </strong>Rodrigo</li>\r\n    </ul>\r\n\r\n    <p>Sharebook</p>\r\n</body>\r\n</html>

            Assert.Contains("Olá Cussa Mitre,", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
            Assert.Contains("<li><strong>Usuário: </strong>Rodrigo</li>", result);
        }

        [Fact]
        public void VerifyEmailBookRequestedParse()
        {
            var vm = new
            {
                Book = book,
                RequestingUser = requestingUser,
                Administrator = administrator
            };

            var result = emailTemplate.GenerateHtmlFromTemplateAsync("BookRequestedTemplate", vm).Result;
            //<!DOCTYPE html>\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <title>Um livro foi solicitado - Sharebook</title>\r\n</head>\r\n<body>\r\n    <p>\r\n        Olá Cussa Mitre,\r\n    </p>\r\n    <p>\r\n        Um livro foi solicitado. Veja mais informações abaixo:\r\n    </p>\r\n\r\n    <ul>\r\n        <li><strong>Livro: </strong>Lord of the Rings</li>\r\n        <li><strong>Donatario: </strong>Walter Vinicius</li>\r\n        <li><strong>Linkedin Donatario:</strong>linkedin.com/walter</li>\r\n        <li><strong>Doador: </strong>Rodrigo</li>\r\n        <li><strong>Linkedin Doador:</strong>linkedin.com/rodrigo</li>\r\n    </ul>\r\n\r\n    <p>Sharebook</p>\r\n</body>\r\n</html>

            Assert.Contains("Olá Cussa Mitre,", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Donatário: </strong>Walter Vinicius</li>", result);
            Assert.Contains("<li><strong>Linkedin Donatário: </strong>linkedin.com/walter</li>", result);
            Assert.Contains("<li><strong>Telefone Donatário: </strong></li>", result);
            Assert.Contains("<li><strong>Email Donatário: </strong>walter@sharebook.com</li>", result);
            Assert.Contains("<li><strong>Doador: </strong>Rodrigo</li>", result);
            Assert.Contains("<li><strong>Linkedin Doador: </strong>linkedin.com/rodrigo</li>", result);
            Assert.Contains("<li><strong>Telefone Doador: </strong></li>", result);
            Assert.Contains("<li><strong>Email Doador: </strong>rodrigo@sharebook.com</li>", result);
        }

        [Fact]
        public void VerifyEmailBookApprovedParse()
        {
            var vm = new { Book = book };

            var result = emailTemplate.GenerateHtmlFromTemplateAsync("BookApprovedTemplate", vm).Result;

            Assert.Contains("<title>Livro aprovado - Sharebook</title>", result);
            Assert.Contains("Olá Rodrigo", result);
            Assert.Contains("O livro Lord of the Rings foi aprovado e já está na nossa vitrine para doação.", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
        }
    }
}
