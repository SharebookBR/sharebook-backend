using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : GenericJob, IJob
    {
        private readonly IBookService _bookService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplate _emailTemplate;

        public RemoveBookFromShowcase(
            IBookService bookService, 
            IJobHistoryRepository jobHistoryRepo,
            IEmailService emailService,
            IEmailTemplate emailTemplate) : base(jobHistoryRepo)
        {

            JobName     = "RemoveBookFromShowcase";
            Description = "Remove o livro da vitrine no dia da decisão. " +
                          "Caso o livro não tenha interessado o mesmo tem a data renovada por mais 10 dias.";
            Interval    = Interval.Dayly;
            Active      = true;
            BestTimeToExecute = new TimeSpan(9, 0, 0);

            _bookService = bookService;
            _emailService = emailService;
            _emailTemplate = emailTemplate;
        }

        public override async Task<JobHistory> WorkAsync()
        {
            var messages = new List<string>();

            var books = await _bookService.GetBooksChooseDateIsTodayOrLateAsync();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                // Só trata livros disponíves
                if (book.Status != BookStatus.Available)
                {
                    messages.Add(string.Format("Livro '{0}' não foi processado porque não está disponível.", book.Title));
                    continue;
                }

                var totalPedidosValidos = book.BookUsers.Count(b => b.Status == DonationStatus.WaitingAction);

                if (totalPedidosValidos > 0)
                {
                    book.Status = BookStatus.AwaitingDonorDecision;
                    messages.Add(string.Format("Livro '{0}' removido da vitrine.", book.Title));
                }
                else
                {
                    book.ChooseDate = DateTime.Today.AddDays(10);
                    messages.Add(string.Format("Livro '{0}' vai ficar +10 dias na vitrine porque ainda não tem interessados. :/", book.Title));
                    await SendEmailAsync(book);
                }

                await _bookService.UpdateAsync(book);
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
            var emailSubject = "SHAREBOOK - RENOVAMOS SUA DOAÇÃO.";

            var vm = new
            {
                DonorName = book.User.Name,
                BookTitle = book.Title,
                BookSlug = book.Slug,
                FacilitatorName = book.UserFacilitator.Name,
                FacilitatorEmail = book.UserFacilitator.Email,
                FacilitatorWhatsapp = book.UserFacilitator.Phone,
                FacilitatorLinkedin = book.UserFacilitator.Linkedin
            };
            var emailBodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateRenewTemplate", vm);

            await _emailService.Send(book.User.Email, book.User.Name, emailBodyHTML, emailSubject, copyAdmins: false, highPriority: true);
        }

        #endregion

    }
}
