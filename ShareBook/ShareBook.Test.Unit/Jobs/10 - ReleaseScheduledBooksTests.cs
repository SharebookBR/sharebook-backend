using Microsoft.Extensions.Logging;
using Moq;
using Sharebook.Jobs;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Test.Unit.Mocks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ShareBook.Test.Unit.Jobs;

public class ReleaseScheduledBooksTests
{
    private readonly Mock<IJobHistoryRepository> _jobHistory = new();
    private readonly Mock<ILoggerFactory> _loggerFactory = new();
    private readonly Mock<IBookService> _bookService = new();
    private readonly Mock<IBookReleaseInterestService> _interestService = new();

    private ReleaseScheduledBooks CreateJob()
        => new(_bookService.Object, _interestService.Object, _jobHistory.Object, TimeProvider.System, _loggerFactory.Object);

    [Fact]
    public async Task ReleasesDueBooksAndThenNotifiesInterestedPeople()
    {
        var book = BookMock.GetLordTheRings();
        _bookService.Setup(s => s.ReleaseDueScheduledBooksAsync()).ReturnsAsync(new List<Book> { book });
        _interestService.Setup(s => s.NotifyReleasedBooksAsync()).ReturnsAsync(3);

        var result = await CreateJob().WorkAsync();

        Assert.True(result.IsSuccess);
        Assert.Contains("1 livro(s) liberado(s)", result.Details);
        Assert.Contains("3 aviso(s)", result.Details);
        _interestService.Verify(s => s.NotifyReleasedBooksAsync(), Times.Once);
    }

    [Fact]
    public async Task StillRetriesPendingNoticesWhenNothingIsDueToday()
    {
        _bookService.Setup(s => s.ReleaseDueScheduledBooksAsync()).ReturnsAsync(new List<Book>());
        _interestService.Setup(s => s.NotifyReleasedBooksAsync()).ReturnsAsync(1);

        var result = await CreateJob().WorkAsync();

        Assert.Contains("Nenhum livro agendado para liberar hoje.", result.Details);
        Assert.Contains("1 aviso(s)", result.Details);
        _interestService.Verify(s => s.NotifyReleasedBooksAsync(), Times.Once);
    }

    [Fact]
    public void RunsAt7amSaoPaulo_BeforeTheNewBooksDigest()
    {
        var job = CreateJob();

        Assert.Equal(new TimeSpan(7, 0, 0), job.BestTimeToExecute);
        Assert.Equal("ReleaseScheduledBooks", job.JobName);
    }
}
