using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sharebook.Jobs
{
    public class LateDonationNotification : GenericJob, IJob
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;
        private readonly IBookService _bookService;

        public LateDonationNotification(IJobHistoryRepository jobHistoryRepo,
            IBookService bookService,
            IEmailService emailService,
            IEmailTemplate emailTemplate) : base(jobHistoryRepo)
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

                htmlTable += string.Format("<TR><TD>{0}<BR>{1}</TD><TD>{2}</TD><TD>{3}</TD><TD>{4}<BR>{5}<BR>{6}<BR>{7}</TD><TD>{8}<BR>{9}<BR>{10}<BR>{11}</TD><TD>{12}</TD></TR>", 
                    book.Title, 
                    book.Status, 
                    book.DaysLate(), 
                    book.TotalInterested(),
                    book.User.Name, book.User.Email, book.User.Phone, book.User.Linkedin,
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

            _emailService.SendToAdmins(emailBodyHTML, emailSubject).Wait();
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
                
                var html = "<p>Bom dia! Aqui é o Sharebook. Vim aqui pra te ajudar a concluir a doação do seu livro.</p>";
                html += "<p>Por favor entre no Sharebook e escolha o ganhador.</p>";
                html += "<p>Para sua conveniência use esse link: <a href='https://www.sharebook.com.br/book/donations' target='_blank'>Minhas doações</a></p>";
                html += "<p>Obrigado. Qualquer dúvida pode entrar em contato com o seu facilitador. É um prazer ajudar. =)</p>";
                html += "<p>Sharebook</p>";

                var emailSubject = "Lembrete do Sharebook";

                details += "E-mail enviado para o usuário: " + donator.Name;

                _emailService.Send(donator.Email, donator.Name, html, emailSubject, copyAdmins: false).Wait();
            }
        }

        #endregion
    }
}
