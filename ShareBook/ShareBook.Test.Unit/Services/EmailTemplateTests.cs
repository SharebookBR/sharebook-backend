using ShareBook.Domain;
using ShareBook.Service;
using ShareBook.Test.Unit.Mocks;
using System.Threading.Tasks;
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
        private ContactUs contactUs;
        private BookUser bookRequested;

        public EmailTemplateTests()
        {
            emailTemplate = new EmailTemplate();

            user = UserMock.GetDonor();

            requestingUser = UserMock.GetGrantee();

            administrator = UserMock.GetAdmin();

            book = BookMock.GetLordTheRings(user);
           
            contactUs = new ContactUs()
            {
                Name = "Rafael Rocha",
                Email = "rafael@sharebook.com.br",
                Message = "At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident",
                Phone = "(11) 954422-2765"
            };

            bookRequested = BookUserMock.GetDonation(book, requestingUser);
        }

        [Fact]
        public async Task VerifyEmailNewBookInsertedParse()
        {
            var vm = new { Book = book };

            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("NewBookInsertedTemplate", vm);
            //<!DOCTYPE html>\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\">\r\n<head>\r\n    <meta charset=\"utf-8\" />\r\n    <title>Novo livro cadastrado - Sharebook</title>\r\n</head>\r\n<body>\r\n    <p>\r\n        Olá Cussa Mitre,\r\n    </p>\r\n    <p>\r\n        Um novo livro foi cadastrado. Veja mais informações abaixo:\r\n    </p>\r\n\r\n    <ul>\r\n        <li><strong>Livro: </strong>Lord of the Rings</li>\r\n        <li><strong>Autor: </strong>J. R. R. Tolkien</li>\r\n        <li><strong>Usuário: </strong>Rodrigo</li>\r\n    </ul>\r\n\r\n    <p>Sharebook</p>\r\n</body>\r\n</html>

            Assert.Contains("Olá Administrador(a),", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
            Assert.Contains("<li><strong>Usuário: </strong>Rodrigo</li>", result);
            Assert.Contains("<li><strong>Usuário: </strong>Rodrigo</li>", result);
            Assert.Contains("https://www.sharebook.com.br/book/form/d9f5fde8-ee7c-4cf5-aa90-35eca3c170b9", result);
        
        }        

        [Fact]
        public async Task VerifyEmailBookApprovedParse()
        {
            var vm = new
            {
                Book = book,
                book.User,
                ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
            };

            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("BookApprovedTemplate", vm);

            Assert.Contains("<title>Livro aprovado - Sharebook</title>", result);
            Assert.Contains("Olá Rodrigo", result);
            Assert.Contains("O livro Lord of the Rings foi aprovado e já está na nossa vitrine para doação.", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
        }

        [Fact]
        public async Task VerifyEmailContactUsNotificationParse()
        {

            var contactUs = new ContactUs()
            {
                Name = "Rafael Rocha",
                Email = "rafael.rochaoliveira@yahoo.com.br"
            };
          

            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("ContactUsNotificationTemplate", contactUs);
            Assert.Contains("Olá, Rafael Rocha", result);

        }

        [Fact]
        public async Task VerifyEmailContactUsTemplateParse()
        {
            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("ContactUsTemplate", contactUs);

            Assert.Contains("<div class=\"field-label\">👤 Nome</div>", result);
            Assert.Contains("<div class=\"field-value\">Rafael Rocha</div>", result);
            Assert.Contains("<div class=\"field-value\">rafael@sharebook.com.br</div>", result);
            Assert.Contains("<div class=\"field-value\">(11) 954422-2765</div>", result);
            Assert.Contains("<div class=\"field-value\">At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident</div>", result);

        }
    }
}