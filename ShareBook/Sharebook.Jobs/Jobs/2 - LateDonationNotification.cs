using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharebook.Jobs;

public class LateDonationNotification : GenericJob, IJob
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IBookService _bookService;

    private readonly int maxLateDonationDays;
    private readonly IConfiguration _configuration;

    private readonly MailSenderHighPriorityQueue _mailSenderHighPriorityQueue;

    public LateDonationNotification(IJobHistoryRepository jobHistoryRepo,
        IBookService bookService,
        IEmailService emailService,
        IEmailTemplate emailTemplate, IConfiguration configuration,
        MailSenderHighPriorityQueue mailSenderHighPriorityQueue) : base(jobHistoryRepo)
    {
        JobName     = "LateDonationNotification";
        Description = "Notifica o facilitador e doador com lista de doações em atraso " +
                        "ordenado pelo mais atrasado.";
        Interval    = Interval.Dayly;
        Active      = true;
        BestTimeToExecute = new TimeSpan(10, 0, 0);

        _bookService = bookService;
        _emailService = emailService;
        _emailTemplate = emailTemplate;

        _configuration = configuration;
        maxLateDonationDays = int.Parse(_configuration["SharebookSettings:MaxLateDonationDays"]);

        _mailSenderHighPriorityQueue = mailSenderHighPriorityQueue;
    }

    public override JobHistory Work()
    {
        var status = _bookService.GetStats();
        var booksLate = _bookService.GetBooksChooseDateIsLate();
        var donators = GetDistinctDonators(booksLate);

        var details = $"Encontradas {booksLate.Count} doações em atraso de {donators.Count} doadores distintos.";
        if (booksLate.Count > 0){
            SendEmailAdmin(booksLate, status);
            SendEmailDonators(donators, ref details);
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

    private void SendEmailAdmin(IList<Book> booksLate, BookStatsDTO status)
    {
        var htmlTable = "<TABLE border=1 cellpadding=3 cellspacing=0><TR bgcolor='#ffff00'><TD><b>LIVRO</b></TD><TD><b>DIAS DE <BR>ATRASO</b></TD><TD><b>TOTAL <br>INTERESSADOS</b></TD><TD><b>DOADOR</b></TD><TD><b>FACILITADOR</b></TD><TD><b>ANOTAÇÕES</b></TD></TR>";

        foreach (var book in booksLate)
        {
            var notes = book.FacilitatorNotes?.Replace("\n", "<BR>");

            var whatsappLink = GetWhatsappLink(book.User.Phone);

            htmlTable += string.Format("<TR><TD>{0}<BR>{1}</TD><TD>{2}</TD><TD>{3}</TD><TD>{4}<BR>{5}<BR>{6}<BR>{7}</TD><TD>{8}<BR>{9}<BR>{10}<BR>{11}</TD><TD>{12}</TD></TR>", 
                book.Title, 
                book.Status, 
                book.DaysLate(), 
                book.TotalInterested(),
                book.User.Name, book.User.Email, whatsappLink, book.User.Linkedin,
                book.UserFacilitator.Name, book.UserFacilitator.Email, book.UserFacilitator.Phone, book.UserFacilitator.Linkedin,
                notes);
        }

        htmlTable += "</TABLE>";

        var emailSubject = "SHAREBOOK - STATUS DO DIA.";

        var vm = new { 
            htmlTable, 
            totalWaitingApproval = status.TotalWaitingApproval,
            totalLate = booksLate.Count,
            totalOk = status.TotalOk
        };
        var emailBodyHTML = _emailTemplate.GenerateHtmlFromTemplateAsync("LateDonationNotification", vm).Result;

        _mailSenderHighPriorityQueue.SendToAdmins(emailBodyHTML, emailSubject);
    }

    private string GetWhatsappLink(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return "";

        string justNumbers = new String(phone.Where(Char.IsDigit).ToArray());
        string link = $"<a href='https://wa.me/55{justNumbers}'>{phone}</a>";

        return link;
    }

    private void SendEmailDonators(IList<User> donators, ref string details)
    {
        foreach (var donator in donators)
        {
            if (!donator.Active)
            {
                details += "E-mail NÃO enviado para o usuário: " + donator.Name + " porque está INATIVO.";
                continue;
            }

            if (donator.HasAbandonedDonation(maxLateDonationDays))
                SendEmailDonatorHard(donator);
            else
                SendEmailDonatorSoft(donator);
                
            details += "E-mail enviado para o usuário: " + donator.Name;
        }
    }

    private void SendEmailDonatorHard(User donator)
    {
        var html = $"<p>Bom dia. Consta em nosso sistema que você tem uma doação abandonada no sharebook com mais de {maxLateDonationDays} dias de atraso.</p>";
        html += "<p>Essa é uma situação grave porque temos muitos usuários aguardando sua decisão. Pessoas humildes que desejam e precisam do livro que vc se propôs a doar em nosso app.</p>";
        html += "<p>Para sua conveniência use esse link para <strong>concluir</strong> ou <strong>cancelar</strong>: <a href='https://www.sharebook.com.br/book/donations' target='_blank'>Minhas doações</a></p>";
        html += "<p>Esse é nosso último aviso. Caso não responda, vamos considerar que a doação foi abandonada e sua conta será bloqueada em nosso sistema.</p>";
            
        html += "<p>Conto com sua compreensão e colaboração.</p>";
        html += "<p>Sinceramente,</p>";
        html += "<p>Sharebook</p>";

        var emailSubject = "Doação abandonada no Sharebook. Urgente!";

        _emailService.Send(donator.Email, donator.Name, html, emailSubject, copyAdmins: true).Wait();
    }

    private void SendEmailDonatorSoft(User donator)
    {
        var html = "<p>Bom dia! Aqui é o Sharebook. Vim aqui pra te ajudar a concluir a doação do seu livro.</p>";
        html += "<p>Por favor entre no Sharebook e escolha o ganhador.</p>";
        html += "<p>Para sua conveniência use esse link: <a href='https://www.sharebook.com.br/book/donations' target='_blank'>Minhas doações</a></p>";
        html += "<p>Obrigado. Qualquer dúvida pode entrar em contato com o seu facilitador. É um prazer ajudar. =)</p>";
        html += "<p>Sharebook</p>";

        var emailSubject = "Lembrete do Sharebook";

        _emailService.Send(donator.Email, donator.Name, html, emailSubject, copyAdmins: false).Wait();
    }

    #endregion
}
