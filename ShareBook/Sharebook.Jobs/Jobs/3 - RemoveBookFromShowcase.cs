using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : GenericJob, IJob
    {

        public RemoveBookFromShowcase(IJobHistoryReposiory jobHistoryRepo) : base(jobHistoryRepo)
        {

            JobName     = "RemoveBookFromShowcase";
            Description = "Remove o livro da vitrine no dia da decisão. " +
                          "Caso o livro não tenha interessado o mesmo tem a data renovada por mais 10 dias.";
            Interval    = Interval.Dayly;
            Active      = false;

        }

        public override JobHistory Work()
        {
            // TODO: implementar trabalho do job aqui.

            return new JobHistory()
            {
                JobName = JobName,
                IsSuccess = true,
                Details = ""
            };
        }
    }
}
