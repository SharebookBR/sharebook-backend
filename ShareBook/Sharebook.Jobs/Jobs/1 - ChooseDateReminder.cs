using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;

namespace Sharebook.Jobs
{
    public class ChooseDateReminder : GenericJob, IJob
    {
        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;
        private readonly IBookService _bookService;

        public ChooseDateReminder(
            IJobHistoryRepository jobHistoryRepo,
            IBookService bookService,
            IEmailService emailService, 
            IEmailTemplate emailTemplate
            ) : base(jobHistoryRepo)
        {
            JobName = "ChooseDateReminder";
            Description = "Notifica o doador, com um lembrete amigável, no dia da doação. " +
                          "Com cópia para o facilitador.";
            Interval = Interval.Dayly;
            Active = false;
            BestTimeToExecute = new TimeSpan(9, 0, 0);

            _bookService = bookService;
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public override JobHistory Work()
        {
            var messages = new List<string>();

            var books = _bookService.GetBooksChooseDateIsToday();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                if (book.Status() == BookStatus.Available && book.BookUsers.Count > 0)
                {
                    SendEmail(book);
                    messages.Add(string.Format("Lembrete amigável enviado para '{0}' referente ao livro '{1}'.", book.User.Name, book.Title));
                }
                else
                {
                    messages.Add(string.Format("Lembrete amigável NÃO enviado para '{0}' referente ao livro '{1}'. Livro não está disponível ou não tem interessados.", book.User.Name, book.Title));
                }
            }

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", messages.ToArray())
            };
        }


        #region métodos privados de apoio

        private void SendEmail(Book book)
        {
            var emailSubject = "SHAREBOOK - É HOJE!";

            var vm = new
            {
                DonorName = book.User.Name,
                BookTitle = book.Title,
                FacilitatorName = book.UserFacilitator.Name,
                FacilitatorEmail = book.UserFacilitator.Email,
                FacilitatorWhatsapp = book.UserFacilitator.Phone,
                FacilitatorLinkedin = book.UserFacilitator.Linkedin
            };
            var emailBodyHTML = _emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderTemplate", vm).Result;

            // TODO: não enviar cópia para admins quando o job já estiver bem testado e amadurecido.
            var copyAdmins = true;
            _emailService.Send(book.User.Email, book.User.Name, emailBodyHTML, emailSubject, copyAdmins);
        }

        #endregion



    }
}
