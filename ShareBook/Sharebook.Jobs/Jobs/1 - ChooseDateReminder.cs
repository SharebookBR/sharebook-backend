using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;

namespace Sharebook.Jobs
{
    public class ChooseDateReminder : GenericJob, IJob
    {
        private IBookRepository _bookRepo;
        private IEmailService _emailService;
        private IEmailTemplate _emailTemplate;

        public ChooseDateReminder(
            IJobHistoryReposiory jobHistoryRepo, 
            IBookRepository bookRepository,
            IEmailService emailService, 
            IEmailTemplate emailTemplate
            ) : base(jobHistoryRepo)
        {
            JobName = "ChooseDateReminder";
            Description = "Notifica o doador, com um lembrete amigável, no dia da doação. " +
                          "Com cópia para o facilitador.";
            Interval = Interval.Dayly;
            Active = true;

            _bookRepo = bookRepository;
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public override JobHistory Work()
        {
            var messages = new List<string>();

            var books = FindBooks();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                if (book.Status() == BookStatus.Available)
                {
                    SendEmail(book);
                    messages.Add(string.Format("Lembrete amigável enviado para '{0}' referente ao livro '{1}'.", book.User.Name, book.Title));
                }
                else
                {
                    messages.Add(string.Format("Lembrete amigável NÃO enviado para '{0}' referente ao livro '{1}'. Livro não está disponível.", book.User.Name, book.Title));
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



        private List<Book> FindBooks()
        {
            // limite eh o dia de hoje.
            DateTime startDateTime = DateTime.Today; //Today at 00:00:00
            DateTime endDateTime = DateTime.Today.AddDays(1).AddTicks(-1); //Today at 23:59:59

            // livros em que o choosedate eh hoje.
            var books = _bookRepo
            .Get().Include(x => x.User).Include(x => x.BookUsers).Include(x => x.UserFacilitator)
            .Where(x =>
                x.ChooseDate >= startDateTime &&
                x.ChooseDate <= endDateTime &&
                x.BookUsers.Count > 0
            ).ToList();

            return books;
        }

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
