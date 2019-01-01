using System;
namespace Sharebook.Jobs
{
    public class LateDonationNotification : IJob
    {
        public string Name { get; }

        public LateDonationNotification()
        {
            Name = "LateDonationNotification";
        }

        public bool HasWork()
        {
            return true;
        }

        public bool Execute()
        {
            //throw new Exception("Erro teste");
            return true;
        }
    }
}
