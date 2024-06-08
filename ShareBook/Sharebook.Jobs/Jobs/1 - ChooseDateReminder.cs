using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            Active = true;
            BestTimeToExecute = new TimeSpan(9, 0, 0);

            _bookService = bookService;
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public override async Task<JobHistory> WorkAsync()
        {
            var messages = new List<string>();

            var books = await _bookService.GetBooksChooseDateIsTodayAsync();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                if (book.BookUsers.Count > 0)
                {
                    await SendEmailAsync(book);
                    messages.Add(string.Format("Lembrete amigável enviado para '{0}' referente ao livro '{1}'.", book.User.Name, book.Title));
                }
                else
                {
                    messages.Add(string.Format("Lembrete amigável NÃO enviado para '{0}' referente ao livro '{1}'. Livro não tem interessados.", book.User.Name, book.Title));
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

        private async Task SendEmailAsync(Book book)
        {
            var emailSubject = "Hoje é o dia de escolher o ganhador!";

            var vm = new
            {
                DonorName = book.User.Name,
                BookTitle = book.Title,
                FacilitatorName = book.UserFacilitator.Name,
                FacilitatorEmail = book.UserFacilitator.Email,
                FacilitatorWhatsapp = book.UserFacilitator.Phone,
                FacilitatorLinkedin = book.UserFacilitator.Linkedin
            };
            var emailBodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderTemplate", vm);

            await _emailService.Send(book.User.Email, book.User.Name, emailBodyHTML, emailSubject, copyAdmins: false, highPriority: true);
        }

        #endregion



    }
}
