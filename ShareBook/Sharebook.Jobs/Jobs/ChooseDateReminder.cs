using System;
namespace Sharebook.Jobs
{
    public class ChooseDateReminder : IJob
    {
        public string Name { get; }

        public ChooseDateReminder()
        {
            Name = "ChooseDateReminder";
        }

        public bool HasWork()
        {
            return false;
        }

        public bool Execute()
        {
            return true;
        }
    }
}
