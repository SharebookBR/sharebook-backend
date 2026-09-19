using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class LateDonationNotification : GenericJob, IJob
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IBookService _bookService;
    private readonly IConfiguration _configuration;

    public const string EmailTemplateName = "LateDonationNotification";
    public const string EmailAdminsSubject = "Status diário das doações";
    public const string EmailDonatorHardSubject = "Último aviso sobre sua doação";
    public const string EmailDonatorSoftSubject = "Só falta escolher quem vai receber";
    public const string ConfigMaxLateDonationDaysKey = "SharebookSettings:MaxLateDonationDays";
    private readonly int maxLateDonationDays;


    public LateDonationNotification(IJobHistoryRepository jobHistoryRepo,
        IBookService bookService,
        IEmailService emailService,
        IEmailTemplate emailTemplate, ILoggerFactory loggerFactory, IConfiguration configuration) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "LateDonationNotification";
        Description = "Notifica administradores e doadores com a lista de doações em atraso " +
                        "ordenado pelo mais atrasado.";
        Interval    = Interval.Dayly;
        Active      = true;
        BestTimeToExecute = new TimeSpan(10, 0, 0);

        _bookService = bookService;
        _emailService = emailService;
        _emailTemplate = emailTemplate;

        _configuration = configuration;
        maxLateDonationDays = int.Parse(_configuration[ConfigMaxLateDonationDaysKey]);

    }

    public override async Task<JobHistory> WorkAsync()
    {
        var status = await _bookService.GetStatsAsync();
        var booksLate = await _bookService.GetBooksChooseDateIsLateAsync();
        var donators = GetDistinctDonators(booksLate);

        var details = $"Encontradas {booksLate.Count} doações em atraso de {donators.Count} doadores distintos.";
        if (booksLate.Count > 0){
            await SendEmailAdminAsync(booksLate, status);
            details += await SendEmailDonatorsAsync(donators);
        }

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = details
        };
    }



    #region métodos privados de apoio

    private List<User> GetDistinctDonators(IList<Book> booksLate)
    {
        return booksLate.Select(b => b.User).Distinct().ToList();
    }

    private async Task SendEmailAdminAsync(IList<Book> booksLate, BookStatsDTO status)
    {
        var htmlTable = "<TABLE border=1 cellpadding=3 cellspacing=0><TR bgcolor='#ffff00'><TD><b>LIVRO</b></TD><TD><b>DIAS DE <BR>ATRASO</b></TD><TD><b>TOTAL <br>INTERESSADOS</b></TD><TD><b>DOADOR</b></TD><TD><b>ANOTAÇÕES</b></TD></TR>";

        foreach (var book in booksLate)
        {
            var notes = book.FacilitatorNotes?.Replace("\n", "<BR>");

            var whatsappLink = GetWhatsappLink(book.User.Phone);

            htmlTable += string.Format("<TR><TD>{0}<BR>{1}</TD><TD>{2}</TD><TD>{3}</TD><TD>{4}<BR>{5}<BR>{6}<BR>{7}</TD><TD>{8}</TD></TR>",
                book.Title, 
                book.Status, 
                book.DaysLate(), 
                book.TotalInterested(),
                book.User.Name, book.User.Email, whatsappLink, book.User.Linkedin,
                notes);
        }

        htmlTable += "</TABLE>";

        var vm = new { 
            htmlTable, 
            totalWaitingApproval = status.TotalWaitingApproval,
            totalLate = booksLate.Count,
            totalOk = status.TotalOk
        };
        var emailBodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync(EmailTemplateName, vm);

        await _emailService.SendToAdminsAsync(emailBodyHTML, EmailAdminsSubject);
    }

    private string GetWhatsappLink(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "";

        string justNumbers = new String(phone.Where(Char.IsDigit).ToArray());
        string link = $"<a href='https://wa.me/55{justNumbers}'>{phone}</a>";

        return link;
    }

    private async Task<string> SendEmailDonatorsAsync(IList<User> donators)
    {
        string details = string.Empty;
        foreach (var donator in donators)
        {
            if (!donator.Active)
            {
                details += "E-mail NÃO enviado para o usuário: " + donator.Name + " porque está INATIVO.";
                continue;
            }

            if (donator.HasAbandonedDonation(maxLateDonationDays))
                await SendEmailDonatorHardAsync(donator);
            else
                await SendEmailDonatorSoftAsync(donator);
                
            details += "E-mail enviado para o usuário: " + donator.Name;
        }
        return details;
    }

    private async Task SendEmailDonatorHardAsync(User donator)
    {
        var html = "<p>Olá!</p>";
        html += $"<p>Sua doação está há mais de {maxLateDonationDays} dias aguardando uma decisão.</p>";
        html += "<p>Entre no Sharebook para escolher o(a) ganhador(a) ou cancelar a doação.</p>";
        html += "<p><a href='https://www.sharebook.com.br/book/donations' target='_blank'><strong>Resolver minha doação</strong></a></p>";
        html += "<p>Este é o último aviso. Se não houver uma resposta, a doação será considerada abandonada e sua conta será bloqueada.</p>";
            
        html += "<p>Se precisar de ajuda, <a href='https://www.sharebook.com.br/contact-us' target='_blank'>fale com a gente</a>.</p>";
        html += "<p>Um abraço,<br>Equipe Sharebook<br><small>Compartilhando conhecimento</small></p>";

        await _emailService.SendAsync(donator.Email, donator.Name, html, EmailDonatorHardSubject, copyAdmins: true, highPriority: true);
    }

    private async Task SendEmailDonatorSoftAsync(User donator)
    {
        var html = "<p>Olá!</p>";
        html += "<p>Tem gente interessada em receber um livro que você colocou para doação. Agora só falta fazer sua escolha.</p>";
        html += "<p><a href='https://www.sharebook.com.br/book/donations' target='_blank'><strong>Escolher ganhador(a)</strong></a></p>";
        html += "<p>Se precisar de ajuda, <a href='https://www.sharebook.com.br/contact-us' target='_blank'>fale com a gente</a>.</p>";
        html += "<p>Valeu por fazer os livros seguirem adiante.</p>";
        html += "<p>Um abraço,<br>Equipe Sharebook<br><small>Compartilhando conhecimento</small></p>";

        await _emailService.SendAsync(donator.Email, donator.Name, html, EmailDonatorSoftSubject, copyAdmins: false, highPriority: true);
    }

    #endregion
}
