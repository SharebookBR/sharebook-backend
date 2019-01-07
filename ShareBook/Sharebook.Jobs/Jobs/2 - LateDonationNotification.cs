using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;

namespace Sharebook.Jobs
{
    public class LateDonationNotification : GenericJob, IJob
    {
        public LateDonationNotification(IJobHistoryReposiory jobHistoryRepo) : base(jobHistoryRepo)
        {
            JobName     = "LateDonationNotification";
            Description = "Notifica o facilitador que uma doação está em atraso. " +
                          "Com cópia para contato@sharebook.com.br.";
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
