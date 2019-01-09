using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : GenericJob, IJob
    {
        private readonly IBookService _bookService;

        public RemoveBookFromShowcase(IBookService bookService, IJobHistoryRepository jobHistoryRepo) : base(jobHistoryRepo)
        {

            JobName     = "RemoveBookFromShowcase";
            Description = "Remove o livro da vitrine no dia da decisão. " +
                          "Caso o livro não tenha interessado o mesmo tem a data renovada por mais 10 dias.";
            Interval    = Interval.Dayly;
            Active      = false;
            BestTimeToExecute = new TimeSpan(9, 0, 0);

        }

        public override JobHistory Work()
        {
            var messages = new List<string>();

            var books = _bookService.GetBooksChooseDateIsToday();

            if (books.Count == 0) messages.Add("Nenhum livro encontrado.");

            foreach (var book in books)
            {
                // Só trata livros disponíves
                if (book.Status() == BookStatus.Available) continue;

                if (book.BookUsers.Count > 0)
                {
                    book.Approved = false;
                    messages.Add(string.Format("Livro '{0}' removido da vitrine.", book.Title));
                }
                else
                {
                    book.ChooseDate = DateTime.Today.AddDays(10);
                    messages.Add(string.Format("Livro '{0}' vai ficar +10 dias na vitrine porque ainda não te interessados. :/", book.Title));
                }

                _bookService.Update(book);
            }

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", messages.ToArray())
            };
        }
    }
}
