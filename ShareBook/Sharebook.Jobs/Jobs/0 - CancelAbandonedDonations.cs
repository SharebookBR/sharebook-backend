using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs
{
    public class CancelAbandonedDonations : GenericJob, IJob
    {
        private readonly IBookService _bookService;
        private readonly IBookUserService _bookUserService;
        private readonly int _maxLateDonationDaysAutoCancel;
        private readonly IConfiguration _configuration;

        public CancelAbandonedDonations(IJobHistoryRepository jobHistoryRepo, IBookService bookService, IBookUserService bookUserService, IConfiguration configuration) : base(jobHistoryRepo)
        {
            JobName     = "CancelAbandonedDonations";
            Description = "Cancela as doações abandonadas.";
            Interval    = Interval.Dayly;
            Active      = true;
            BestTimeToExecute = new TimeSpan(6, 0, 0);

            _bookService = bookService;
            _bookUserService = bookUserService;

            _configuration = configuration;
            _maxLateDonationDaysAutoCancel = int.Parse(_configuration["SharebookSettings:MaxLateDonationDaysAutoCancel"]);
        }

        public override async Task<JobHistory> WorkAsync()
        {
            var booksLate = await _bookService.GetBooksChooseDateIsLateAsync();

            var refDate = DateTime.Today.AddDays(_maxLateDonationDaysAutoCancel * -1);
            var booksAbandoned = booksLate.Where(b => b.ChooseDate < refDate).ToList();

            var details = $"Encontradas {booksAbandoned.Count} doações abandonadas com mais de {_maxLateDonationDaysAutoCancel} dias de atraso.\n\n";
            
            foreach(var book in booksAbandoned)
            {
                var dto = new BookCancelationDTO
                {
                    Book = book,
                    CanceledBy = "ShareBot",
                    Reason = $"Cancelamento automático de doação abandonada. Com mais de {_maxLateDonationDaysAutoCancel} dias de atraso.",
                };

                // TODO: Migrate to async
                _bookUserService.Cancel(dto);
                details += $"Doação do livro {book.Title} foi cancelada.\n";
            }
            
            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = details
            };
        }

    }
}
