using System;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Domain.Enums;
using System.Collections.Generic;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : GenericJob, IJob
    {
        private IJobHistoryReposiory _jobHistoryRepo;

        public RemoveBookFromShowcase(IJobHistoryReposiory jobHistoryRepo)
        {
            _jobHistoryRepo = jobHistoryRepo;

            JobName = "RemoveBookFromShowcase";
            Description = "Remove o livro da vitrine no dia da decisão. " +
                          "Caso o livro não tenha interessado o mesmo tem a data renovada por mais 10 dias.";
            Interval = Interval.Dayly;
            Active = true;

        }

        public bool HasWork()
        {
            return true;            
        }

        public bool Execute()
        {
            return true;
        }

        
    }
}
