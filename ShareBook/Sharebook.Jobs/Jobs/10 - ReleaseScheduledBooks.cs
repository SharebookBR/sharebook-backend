using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class ReleaseScheduledBooks : GenericJob, IJob
{
    private readonly IBookService _bookService;

    public ReleaseScheduledBooks(
        IBookService bookService,
        IJobHistoryRepository jobHistoryRepo,
        TimeProvider timeProvider,
        ILoggerFactory loggerFactory) : base(jobHistoryRepo, loggerFactory, timeProvider)
    {
        JobName = "ReleaseScheduledBooks";
        Description = "Libera livros físicos agendados (Em breve) cuja data de liberação chegou. " +
                      "Roda antes do digest de novos livros (NewBookGetInterestedUsers, 8h) para que ele já inclua os liberados.";
        Interval = Interval.Dayly;
        Active = true;
        BestTimeToExecute = new TimeSpan(7, 0, 0);

        _bookService = bookService;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var released = await _bookService.ReleaseDueScheduledBooksAsync();

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = released.Count == 0
                ? "Nenhum livro agendado para liberar hoje."
                : $"{released.Count} livro(s) liberado(s): " + string.Join(", ", released.Select(b => b.Title))
        };
    }
}
