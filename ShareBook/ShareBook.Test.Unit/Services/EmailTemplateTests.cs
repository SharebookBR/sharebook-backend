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

        public EmailTemplateTests()
        {
            emailTemplate = new EmailTemplate();
        }

        [Fact]
        public void VerifyEmailParse()
        {
            var user = new User()
            {
                Id = new Guid("5489A967-9320-4350-E6FC-08D5CC8498F3"),
                Name = "Walter Vinicius",
                Email = "walter@sharebook.com",
                Profile = Domain.Enums.Profile.User
            };
            var book = new Book()
            {
                Title = "Lord of the Rings",
                Author = "J. R. R. Tolkien",
                Image = "lotr.png",
                ImageBytes = Encoding.UTF8.GetBytes("STRINGBASE64"),
                User = user
            };
            var administrator = new User()
            {
                Id = new Guid("5489A967-AAAA-BBBB-CCCC-08D5CC8498F3"),
                Name = "Cussa Mitre",
                Email = "cussa@sharebook.com",
                Profile = Domain.Enums.Profile.Administrator
            };

            var vm = new
            {
                Book = book,
                Administrator = administrator
            };

            var result = emailTemplate.GenerateHtmlFromTemplateAsync("NewBookInsertedTemplate", vm).Result;

            var expected = "<!DOCTYPE html>\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <title>Novo livro cadastrado - Sharebook</title>\r\n</head>\r\n<body>\r\n    <p>\r\n        Olá Cussa Mitre,\r\n    </p>\r\n    <p>\r\n        Um novo livro foi cadastrado. Veja mais informações abaixo:\r\n    </p>\r\n\r\n    <ul>\r\n        <li><strong>Livro: </strong>Lord of the Rings</li>\r\n        <li><strong>Autor: </strong>J. R. R. Tolkien</li>\r\n        <li><strong>Usuário: </strong>Walter Vinicius</li>\r\n    </ul>\r\n\r\n    <p>Sharebook</p>\r\n</body>\r\n</html>  ";

            Assert.Equal(expected, result);
        }
    }
}
