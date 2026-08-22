using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class ChooseDateReminder : GenericJob, IJob
{
    private readonly IEmailService _emailService;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IBookService _bookService;

    public ChooseDateReminder(
        IJobHistoryRepository jobHistoryRepo,
        ILoggerFactory loggerFactory,
        IBookService bookService,
        IEmailService emailService,
        IEmailTemplate emailTemplate
        ) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "ChooseDateReminder";
        Description = "Notifica o doador com um lembrete amigável no dia da escolha.";
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

        // Agrupa livros por doador para enviar um único e-mail resumido
        var booksByDonor = books
            .Where(b => b.BookUsers.Count > 0)
            .GroupBy(b => b.UserId)
            .ToList();

        var booksWithoutInterested = books.Where(b => b.BookUsers.Count == 0).ToList();

        foreach (var group in booksByDonor)
        {
            var donorBooks = group.ToList();

            if (donorBooks.Count == 1)
            {
                await SendEmailSingleAsync(donorBooks[0]);
            }
            else
            {
                await SendEmailMultipleAsync(donorBooks);
            }

            var titles = string.Join(", ", donorBooks.Select(b => $"'{b.Title}'"));
            messages.Add($"Lembrete amigável enviado para '{donorBooks[0].User.Name}' referente a {donorBooks.Count} livro(s): {titles}.");
        }

        foreach (var book in booksWithoutInterested)
        {
            messages.Add($"Lembrete amigável NÃO enviado para '{book.User.Name}' referente ao livro '{book.Title}'. Livro não tem interessados.");
        }

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = String.Join("\n", messages.ToArray())
        };
    }


    #region métodos privados de apoio

    private async Task SendEmailSingleAsync(Book book)
    {
        var EmailSubject = "Hoje é dia de escolher quem vai receber";

        // O template não usa dados do facilitador. Lê-los aqui só criava
        // NullReferenceException em livro sem facilitador, que é opcional.
        var vm = new
        {
            DonorName = book.User.Name,
            BookTitle = book.Title
        };
        var emailBodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderTemplate", vm);

        await _emailService.SendAsync(book.User.Email, book.User.Name, emailBodyHTML, EmailSubject, copyAdmins: false, highPriority: true);
    }

    private async Task SendEmailMultipleAsync(List<Book> books)
    {
        var donor = books[0].User;
        var EmailSubject = "Hoje é dia de escolher quem vai receber";

        var bookListHtml = "<ul>";
        foreach (var book in books)
        {
            var interestedCount = book.TotalInterested();
            bookListHtml += $"<li><strong>{book.Title}</strong> — {interestedCount} interessado(s)</li>";
        }
        bookListHtml += "</ul>";

        var vm = new
        {
            DonorName = donor.Name,
            BookListHtml = bookListHtml
        };
        var emailBodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync("ChooseDateReminderMultipleTemplate", vm);

        await _emailService.SendAsync(donor.Email, donor.Name, emailBodyHTML, EmailSubject, copyAdmins: false, highPriority: true);
    }

    #endregion



}
