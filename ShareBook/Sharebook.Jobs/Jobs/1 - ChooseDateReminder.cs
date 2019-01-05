using System;
using ShareBook.Domain.Enums;

namespace Sharebook.Jobs
{
    public class ChooseDateReminder : GenericJob, IJob
    {
        public ChooseDateReminder()
        {
            JobName = "ChooseDateReminder";
            Description = "Notifica o doador, com um lembrete amigável, no dia da doação. " +
                          "Com cópia para o facilitador.";
            Interval = Interval.Dayly;
            Active = true;
        }

        public bool HasWork()
        {
            return false;


            // está na hora de rodar o job? Passou o intervalo?
            var DateLimit = GetDateLimitByInterval(Interval);
        }

        public bool Execute()
        {
            return true;
        }

        public DateTime GetDateLimitByInterval(Interval i)
        {
            var result = DateTime.Now;

            switch (i)
            {
                case Interval.Dayly:
                    {
                        result = result.AddDays(-1);
                        break;
                    }
                case Interval.Hourly:
                    {
                        result = result.AddHours(-1);
                        break;
                    }
                case Interval.Weekly:
                    {
                        result = result.AddDays(-7);
                        break;
                    }
            }

            return result;
        }
    }
}
