using ShareBook.Domain;
using ShareBook.Service;
using ShareBook.Test.Unit.Mocks;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Services;

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
    public async Task VerifyEmailNewBookNotifyParse()
    {
        var vm = new
        {
            Name = "Maria",
            BookTitle = "Lord of the Rings",
            BookSlug = "lord-of-the-rings",
            BookImageSlug = "lord-of-the-rings.jpg",
            BackendUrl = "https://api.sharebook.com.br",
            FrontendUrl = "https://www.sharebook.com.br"
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("NewBookNotifyTemplate", vm);

        Assert.Contains("<title>Novo na vitrine: Lord of the Rings</title>", result);
        Assert.Contains("Olá, Maria.", result);
        Assert.Contains("Tem livro novo na vitrine do Sharebook: <strong>Lord of the Rings</strong>.", result);
        Assert.Contains("vale abrir a página e fazer sua solicitação enquanto ele está disponível", result);
        Assert.Contains("https://api.sharebook.com.br/Images/Books/lord-of-the-rings.jpg", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/livros/lord-of-the-rings\"", result);
        Assert.Contains("Ver livro na vitrine", result);
        Assert.DoesNotContain("Um novo livro está disponível para doação no Sharebook", result);
    }

    [Fact]
    public async Task VerifyEmailBookReceivedParse()
    {
        var vm = new
        {
            Book = book,
            User = user,
            WinnerName = "Maria"
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("BookReceivedTemplate", vm);

        Assert.Contains("<html lang=\"pt-br\">", result);
        Assert.Contains("<title>Doação concluída</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("Boa notícia: Maria confirmou o recebimento do livro <strong>Lord of the Rings</strong>.", result);
        Assert.Contains("Sua doação foi concluída. Esse livro saiu da sua estante", result);
        Assert.Contains("<div class=\"info-title\">Resumo da doação</div>", result);
        Assert.Contains("<li><strong>Quem recebeu:</strong> Maria</li>", result);
        Assert.Contains("Cada doação concluída deixa o Sharebook um pouco mais vivo.", result);
        Assert.DoesNotContain("O recebimento do livro", result);
        Assert.DoesNotContain("Detalhes da doação", result);
    }

    [Fact]
    public async Task VerifyEmailPrintedBooksDigestParse()
    {
        var vm = new
        {
            Name = "Maria",
            BookCountMessage = "Hoje chegaram 3 livros novos.",
            BookListHtml = "<tr><td>Lord of the Rings</td></tr>",
            FrontendUrl = "https://www.sharebook.com.br",
            UnsubscribeUrl = "https://www.sharebook.com.br/unsubscribe/token"
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("PrintedBooksDigestTemplate", vm);

        Assert.Contains("<title>Novos livros chegaram à vitrine</title>", result);
        Assert.Contains("<h2>Novos livros chegaram à vitrine</h2>", result);
        Assert.Contains("Olá, Maria.", result);
        Assert.Contains("A vitrine do Sharebook recebeu novos livros físicos hoje.", result);
        Assert.Contains("Hoje chegaram 3 livros novos.", result);
        Assert.Contains("talvez tenha um título esperando justamente por você", result);
        Assert.Contains("<tr><td>Lord of the Rings</td></tr>", result);
        Assert.Contains("href=\"https://www.sharebook.com.br\" class=\"button\"", result);
        Assert.Contains("Ver novidades na vitrine", result);
        Assert.DoesNotContain("Livros novos disponíveis hoje", result);
    }

    [Fact]
    public async Task VerifyEmailEbooksWeeklyDigestParse()
    {
        var vm = new
        {
            Name = "Maria",
            EbookCountMessage = "Esta semana chegaram 5 livros digitais.",
            EbookListHtml = "<tr><td>Memórias Póstumas de Brás Cubas</td></tr>",
            AdditionalEbooksMessage = "<p>Tem mais títulos esperando por você.</p>",
            FrontendUrl = "https://www.sharebook.com.br",
            UnsubscribeUrl = "https://www.sharebook.com.br/unsubscribe/token"
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("EbooksWeeklyDigestTemplate", vm);

        Assert.Contains("<title>Novos livros digitais para ler esta semana</title>", result);
        Assert.Contains("<h2>Novos livros digitais para ler esta semana</h2>", result);
        Assert.Contains("Olá, Maria.", result);
        Assert.Contains("A biblioteca digital do Sharebook ganhou novidades esta semana.", result);
        Assert.Contains("Esta semana chegaram 5 livros digitais.", result);
        Assert.Contains("Escolha um título, abra a página e comece a leitura quando quiser.", result);
        Assert.Contains("<tr><td>Memórias Póstumas de Brás Cubas</td></tr>", result);
        Assert.Contains("<p>Tem mais títulos esperando por você.</p>", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/livros-digitais/novidades\" class=\"button\"", result);
        Assert.Contains("Ver novidades digitais", result);
        Assert.DoesNotContain("Livros digitais novos esta semana", result);
    }

    [Fact]
    public async Task VerifyEmailBookApprovedParse()
    {
        book.Slug = "lord-of-the-rings";
        book.ChooseDate = new System.DateTime(2026, 10, 26);
        var vm = new
        {
            Book = book,
            book.User,
            ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("BookApprovedTemplate", vm);

        Assert.Contains("<title>Seu livro está na vitrine!</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("<strong>Lord of the Rings</strong>, de J. R. R. Tolkien, já está na vitrine do Sharebook.", result);
        Assert.Contains("Sempre que chegar uma nova solicitação, a gente te avisa por e-mail.", result);
        Assert.Contains("Data de escolha: 26/10/2026", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/livros/lord-of-the-rings\"", result);
        Assert.Contains("Obrigado por compartilhar conhecimento.", result);
        Assert.DoesNotContain("Detalhes do livro", result);
    }

    [Fact]
    public async Task VerifyEmailEbookApprovedParse()
    {
        book.Type = global::ShareBook.Domain.Enums.BookType.Eletronic;
        book.Slug = "lord-of-the-rings";

        var vm = new
        {
            Book = book,
            book.User,
            ChooseDate = book.ChooseDate?.ToString("dd/MM/yyyy")
        };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("EbookApprovedTemplate", vm);

        Assert.Contains("<title>Seu livro digital está no ar!</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("<strong>Lord of the Rings</strong>, de J. R. R. Tolkien, já está disponível no Sharebook.", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/livros/lord-of-the-rings\"", result);
        Assert.Contains("Obrigado por compartilhar conhecimento.", result);
        Assert.DoesNotContain("sua obra", result);
        Assert.DoesNotContain("Detalhes do livro", result);
        Assert.DoesNotContain("ganhador", result);
        Assert.DoesNotContain("vitrine para doação", result);
    }

    [Fact]
    public async Task VerifyEmailEbookWaitingApprovalParse()
    {
        book.Type = global::ShareBook.Domain.Enums.BookType.Eletronic;

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("EbookWaitingApprovalTemplate", book);

        Assert.Contains("<title>Recebemos seu livro digital para revisão</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("Que alegria receber <strong>Lord of the Rings</strong>, de J. R. R. Tolkien.", result);
        Assert.Contains("Quando terminarmos, você recebe outro e-mail.", result);
        Assert.Contains("Obrigado por compartilhar conhecimento.", result);
        Assert.DoesNotContain("Detalhes do livro", result);
        Assert.DoesNotContain("não é preciso fazer nada", result);
        Assert.DoesNotContain("vitrine", result);
        Assert.DoesNotContain("outras pessoas possam visualizar", result);
    }

    [Fact]
    public async Task VerifyEmailWaitingApprovalParse()
    {
        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("WaitingApprovalTemplate", book);

        Assert.Contains("<title>Recebemos seu livro para revisão</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("Que alegria receber <strong>Lord of the Rings</strong>, de J. R. R. Tolkien.", result);
        Assert.Contains("Quando terminarmos, você recebe outro e-mail.", result);
        Assert.Contains("Na data de escolha, é você quem decide quem vai ganhar.", result);
        Assert.Contains("Obrigado por compartilhar conhecimento.", result);
        Assert.DoesNotContain("Detalhes do livro", result);
    }

    [Fact]
    public async Task VerifyEmailChooseDateReminderParse()
    {
        var vm = new { DonorName = "Rodrigo", BookTitle = "Lord of the Rings", RequestsText = "3 solicitações" };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderTemplate", vm);

        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("<strong>Lord of the Rings</strong> recebeu 3 solicitações, e hoje é você quem escolhe quem vai ganhar.", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/book/donations\"", result);
        Assert.Contains("você recebe por e-mail os dados da pessoa ganhadora", result);
        Assert.DoesNotContain("Entre em contato", result);
    }

    [Fact]
    public async Task VerifyEmailChooseDateReminderMultipleParse()
    {
        var vm = new { DonorName = "Rodrigo", BookListHtml = "<ul><li><strong>Lord of the Rings</strong>: 1 solicitação</li></ul>" };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderMultipleTemplate", vm);

        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("hoje é a data de escolha dos livros abaixo", result);
        Assert.Contains("<li><strong>Lord of the Rings</strong>: 1 solicitação</li>", result);
        Assert.Contains("Depois de cada escolha, você recebe por e-mail os dados da pessoa ganhadora", result);
        Assert.DoesNotContain("Entre em contato", result);
    }

    [Fact]
    public async Task VerifyEmailChooseDateRenewParse()
    {
        var vm = new { DonorName = "Rodrigo", BookTitle = "Lord of the Rings", BookSlug = "lord-of-the-rings", ChooseDate = "05/10/2026" };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateRenewTemplate", vm);

        Assert.Contains("<title>Mais 10 dias na vitrine</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("<strong>Lord of the Rings</strong> ainda não recebeu solicitações", result);
        Assert.Contains("A nova data de escolha é <strong>05/10/2026</strong>.", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/livros/lord-of-the-rings\"", result);
    }

    [Fact]
    public async Task VerifyEmailLateDonationDonorSoftParse()
    {
        var vm = new { DonorName = "Rodrigo", BookListHtml = EmailText.BookRequestsListHtml(new[] { book }) };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("LateDonationDonorSoftTemplate", vm);

        Assert.Contains("<title>Só falta escolher quem vai receber</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("<li><strong>Lord of the Rings</strong>: 1 solicitação</li>", result);
        Assert.Contains("href=\"https://www.sharebook.com.br/book/donations\"", result);
        Assert.Contains("você também pode cancelar a doação", result);
        Assert.Contains("fale com a gente", result);
    }

    [Fact]
    public async Task VerifyEmailLateDonationDonorHardParse()
    {
        var vm = new { DonorName = "Rodrigo", MaxLateDonationDays = 5, BookListHtml = EmailText.BookRequestsListHtml(new[] { book }) };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("LateDonationDonorHardTemplate", vm);

        Assert.Contains("<title>Último aviso sobre sua doação</title>", result);
        Assert.Contains("Olá, Rodrigo!", result);
        Assert.Contains("Faz mais de 5 dias que a data de escolha passou", result);
        Assert.Contains("<li><strong>Lord of the Rings</strong>: 1 solicitação</li>", result);
        Assert.Contains("a doação será cancelada automaticamente", result);
        Assert.Contains("Resolver minha doação", result);
        Assert.DoesNotContain("bloquead", result);
        Assert.Contains("fale com a gente", result);
    }

    [Fact]
    public async Task VerifyEmailBookNoticeDeclinedUsersParse()
    {
        var vm = new { BookTitle = "Lord of the Rings" };

        var result = await emailTemplate.GenerateHtmlFromTemplateAsync("BookNoticeDeclinedUsersTemplate", vm);

        Assert.Contains("<title>Outra pessoa receberá este livro</title>", result);
        Assert.Contains("A escolha da doação do livro <strong>Lord of the Rings</strong> foi concluída", result);
        Assert.Contains("Sua solicitação ajuda", result);
        Assert.Contains("aquele livro encontrou leitores interessados", result);
        Assert.Contains("solicitar outros livros disponíveis", result);
        Assert.Contains("Ver vitrine do Sharebook", result);
        Assert.Contains("href=\"https://www.sharebook.com.br\"", result);
        Assert.DoesNotContain("lang=\"en\"", result);
        Assert.DoesNotContain("doador(a)", result);
        Assert.DoesNotContain("ganhador(a)", result);
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
        Assert.Contains("Olá, Rafael.", result);

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
            "LateDonationDonorSoftTemplate.html",
            "LateDonationDonorHardTemplate.html",
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
