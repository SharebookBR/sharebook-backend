using ShareBook.Domain;
using ShareBook.Service;
using ShareBook.Test.Unit.Mocks;
using System.IO;
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

            Assert.Contains("Há um novo livro aguardando revisão.", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
            Assert.Contains("<li><strong>Pessoa responsável: </strong>Rodrigo</li>", result);
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

            Assert.Contains("<title>Seu livro foi aprovado</title>", result);
            Assert.Contains("Olá Rodrigo", result);
            Assert.Contains("O livro Lord of the Rings foi aprovado e já está na nossa vitrine para doação.", result);
            Assert.Contains("<li><strong>Livro: </strong>Lord of the Rings</li>", result);
            Assert.Contains("<li><strong>Autor: </strong>J. R. R. Tolkien</li>", result);
        }

        [Fact]
        public async Task VerifyEmailEbookApprovedParse()
        {
            book.Type = global::ShareBook.Domain.Enums.BookType.Eletronic;

            var vm = new
            {
                Book = book,
                book.User,
                ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
            };

            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("EbookApprovedTemplate", vm);

            Assert.Contains("<title>Seu livro digital foi aprovado</title>", result);
            Assert.Contains("Olá, Rodrigo", result);
            Assert.Contains("Seu livro digital Lord of the Rings foi aprovado e já está disponível no Sharebook.", result);
            Assert.Contains("Leitores já podem acessar e baixar sua obra.", result);
            Assert.DoesNotContain("ganhador", result);
            Assert.DoesNotContain("vitrine para doação", result);
        }

        [Fact]
        public async Task VerifyEmailEbookWaitingApprovalParse()
        {
            book.Type = global::ShareBook.Domain.Enums.BookType.Eletronic;

            var result = await emailTemplate.GenerateHtmlFromTemplateAsync("EbookWaitingApprovalTemplate", book);

            Assert.Contains("<title>Recebemos seu livro digital para revisão</title>", result);
            Assert.Contains("Recebemos o livro digital Lord of the Rings", result);
            Assert.Contains("Você receberá outro e-mail quando a revisão terminar.", result);
            Assert.DoesNotContain("vitrine", result);
            Assert.DoesNotContain("outras pessoas possam visualizar", result);
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

            Assert.Contains("<div class=\"field-label\">Nome</div>", result);
            Assert.Contains("<div class=\"field-value\">Rafael Rocha</div>", result);
            Assert.Contains("<div class=\"field-value\">rafael@sharebook.com.br</div>", result);
            Assert.Contains("<div class=\"field-value\">(11) 954422-2765</div>", result);
            Assert.Contains("<div class=\"field-value\">At vero eos et accusamus et iusto odio dignissimos ducimus qui blanditiis praesentium voluptatum deleniti atque corrupti quos dolores et quas molestias excepturi sint occaecati cupiditate non provident</div>", result);

        }

        [Fact]
        public void VerifyCanonicalEmailFooters()
        {
            var templatesFolder = Path.Combine(System.AppContext.BaseDirectory, "Email", "Templates");

            var transactionalTemplates = new[]
            {
                "BookApprovedTemplate.html",
                "BookCanceledNoticeUsersTemplate.html",
                "BookCanceledTemplate.html",
                "BookDonatedNotifyDonorTemplate.html",
                "BookDonatedTemplate.html",
                "BookNoticeDeclinedUsersTemplate.html",
                "BookNoticeDonorTemplate.html",
                "BookNoticeInterestedTemplate.html",
                "BookReceivedTemplate.html",
                "BookTrackingNumberNoticeWinnerTemplate.html",
                "ChooseDateReminderMultipleTemplate.html",
                "ChooseDateReminderTemplate.html",
                "ChooseDateRenewTemplate.html",
                "ContactUsNotificationTemplate.html",
                "EbookApprovedTemplate.html",
                "EbookWaitingApprovalTemplate.html",
                "ForgotPasswordTemplate.html",
                "NewBookNotifyTemplate.html",
                "ParentAprovedNotifyUser.html",
                "RequestParentAproval.html",
                "WaitingApprovalTemplate.html"
            };

            var newsletterTemplates = new[]
            {
                "EbooksWeeklyDigestTemplate.html",
                "PrintedBooksDigestTemplate.html"
            };

            var internalTemplates = new[]
            {
                "AnonymizeNotifyAdms.html",
                "ContactUsTemplate.html",
                "LateDonationNotification.html",
                "NewBookInsertedTemplate.html"
            };

            Assert.Equal(
                transactionalTemplates.Length + newsletterTemplates.Length + internalTemplates.Length,
                Directory.GetFiles(templatesFolder, "*.html").Length);

            foreach (var template in transactionalTemplates)
            {
                var html = ReadTemplate(templatesFolder, template);
                Assert.Contains("https://www.sharebook.com.br/contact-us", html);
                Assert.Contains("fale com a gente", html);
                Assert.Contains("Um abraço", html);
                AssertCanonicalBranding(html);
            }

            foreach (var template in newsletterTemplates)
            {
                var html = ReadTemplate(templatesFolder, template);
                Assert.Contains("Um abraço", html);
                Assert.Contains("Cancelar inscrição", html);
                Assert.DoesNotContain("fale com a gente", html);
                AssertCanonicalBranding(html);
            }

            foreach (var template in internalTemplates)
            {
                var html = ReadTemplate(templatesFolder, template);
                Assert.DoesNotContain("Um abraço", html);
                Assert.DoesNotContain("fale com a gente", html);
                AssertCanonicalBranding(html);
            }
        }

        private static string ReadTemplate(string templatesFolder, string template)
        {
            var html = File.ReadAllText(Path.Combine(templatesFolder, template));
            var normalized = html.ToLowerInvariant();

            Assert.DoesNotContain("facilitador", normalized);
            Assert.DoesNotContain("instagram.com", normalized);
            Assert.DoesNotContain("linkedin.com/company/sharebook-br", normalized);
            Assert.DoesNotContain("facebook.com/sharebookbr", normalized);
            Assert.DoesNotContain("mit license", normalized);
            Assert.DoesNotContain("pessoas humildes", normalized);
            Assert.DoesNotContain("para sua conveniência", normalized);
            Assert.DoesNotContain("sharebot", normalized);
            Assert.DoesNotContain(":-)", normalized);
            Assert.DoesNotContain("tomar um café", normalized);
            Assert.DoesNotContain("realmente precisa", normalized);
            Assert.DoesNotContain("mais precisar", normalized);
            Assert.DoesNotContain("nossos agradecimentos", normalized);

            return html;
        }

        private static void AssertCanonicalBranding(string html)
        {
            Assert.Contains("Equipe Sharebook", html);
            Assert.Contains("Compartilhando conhecimento", html);
        }
    }
}
