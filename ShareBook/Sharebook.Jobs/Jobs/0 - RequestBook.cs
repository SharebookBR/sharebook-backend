using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sharebook.Jobs
{
    public class RequestBook : GenericJob, IJob
    {
        private readonly IBookUserRepository _bookUserRepository;
        private readonly IBookUsersEmailService _bookUsersEmailService;
        private int totalEmails;

        public RequestBook(
            IJobHistoryRepository jobHistoryRepo, IBookUserRepository bookUserRepository,
            IBookUsersEmailService bookUsersEmailService
            ) : base(jobHistoryRepo)
        {
            JobName = "RequestBookNotify";
            Description = "Os requests de livros são enviados para o dono do livro, agrupando por intervalo e dono de livro, assim reduzindo a carga de emails enviados.";
            Interval = Interval.Hourly;
            Active = true;
            BestTimeToExecute = null;

            _bookUserRepository = bookUserRepository;
            _bookUsersEmailService = bookUsersEmailService;
        }

        public override JobHistory Work()
        {

            DateTime timeReference = DateTime.Now.AddHours(-1);
            List<BookUser> bookUsers = _bookUserRepository.Get().Where(b => b.CreationDate >= timeReference && b.CreationDate <= DateTime.Now).ToList();
            if (bookUsers.Count > 0)
            {
                var emailsList = new List<KeyValuePair<Guid, List<BookUser>>>();

                //Separa por chave e valor numa lista
                foreach (BookUser bookUser in bookUsers)
                {
                    if (emailsList.Any(x => x.Key == bookUser.User.Id))
                    {
                        var email = emailsList.Find(x => x.Key == bookUser.User.Id);
                        email.Value.Add(bookUser);
                    }
                    else
                    {
                        emailsList.Add(new KeyValuePair<Guid, List<BookUser>>(bookUser.User.Id, new List<BookUser>()));
                        var email = emailsList.Find(x => x.Key == bookUser.User.Id);
                        email.Value.Add(bookUser);
                    }
                }
                totalEmails = emailsList.Count;
                foreach (var email in emailsList)
                {
                    _bookUsersEmailService.SendEmailBookRequested(email.Value).Wait();
                }
            }

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = String.Join("\n", $"{totalEmails} e-mails enviados.")
            };
        }
    }
}