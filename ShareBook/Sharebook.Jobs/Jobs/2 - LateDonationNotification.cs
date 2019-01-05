using System;
using ShareBook.Domain.Enums;

namespace Sharebook.Jobs
{
    public class LateDonationNotification : GenericJob, IJob
    {
        public LateDonationNotification()
        {
            JobName = "LateDonationNotification";
            Description = "Notifica o facilitador que uma doação está em atraso. " +
                          "Com cópia para contato@sharebook.com.br.";
            Interval = Interval.Dayly;
            Active = false;
        }

        public bool HasWork()
        {
            return false;
        }

        public bool Execute()
        {
            //throw new Exception("Erro teste");
            return true;
        }
    }
}
