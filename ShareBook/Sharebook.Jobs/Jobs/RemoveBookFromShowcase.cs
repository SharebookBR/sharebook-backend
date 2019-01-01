using System;

namespace Sharebook.Jobs
{
    public class RemoveBookFromShowcase : IJob
    {
        public string Name { get; }

        public RemoveBookFromShowcase()
        {
            Name = "RemoveBookFromShowcase";
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
