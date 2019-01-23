using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;

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
            Description = "Notifica o facilitador com lista de doações em atraso " +
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
            var messages = new List<string>();

            var books = _bookService.GetBooksChooseDateIsLate();
            var booksLate = new List<Book>();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                if (book.Status() == BookStatus.Available || book.Status() == BookStatus.Invisible)
                {
                    booksLate.Add(book);
                    messages.Add(string.Format("Livro '{0}' adicionado na lista de atrasados.", book.Title));
                }
                else
                {
                    messages.Add(string.Format("Livro '{0}' NÃO adicionado na lista de atrasados porque seu status é {1}", book.Title, book.Status()));
                }
            }

            if (booksLate.Count > 0) SendEmail(booksLate);

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", messages.ToArray())
            };
        }

        #region métodos privados de apoio

        private void SendEmail(List<Book> books)
        {
            var htmlTable = "<TABLE border=1 cellpadding=3 cellspacing=0><TR bgcolor='#ffff00'><TD><b>LIVRO</b></TD><TD><b>DIAS NA <BR>VITRINE</b></TD><TD><b>TOTAL <br>INTERESSADOS</b></TD><TD><b>DOADOR</b></TD><TD><b>FACILITADOR</b></TD></TR>";

            foreach (var book in books)
            {
                htmlTable += string.Format("<TR><TD>{0}<BR>{1}</TD><TD>{2}</TD><TD>{3}</TD><TD>{4}<BR>{5}<BR>{6}<BR>{7}</TD><TD>{8}<BR>{9}<BR>{10}<BR>{11}</TD></TR>", 
                    book.Title, 
                    book.Status(), 
                    book.DaysInShowcase(), 
                    book.TotalInterested(),
                    book.User.Name, book.User.Email, book.User.Phone, book.User.Linkedin,
                    book.UserFacilitator.Name, book.UserFacilitator.Email, book.UserFacilitator.Phone, book.UserFacilitator.Linkedin);
            }

            htmlTable += "</TABLE>";

            var emailSubject = "SHAREBOOK - LISTA DE DOAÇÕES EM ATRASO.";

            var vm = new { htmlTable };
            var emailBodyHTML = _emailTemplate.GenerateHtmlFromTemplateAsync("LateDonationNotification", vm).Result;

            _emailService.SendToAdmins(emailBodyHTML, emailSubject);
        }

        #endregion
    }
}
