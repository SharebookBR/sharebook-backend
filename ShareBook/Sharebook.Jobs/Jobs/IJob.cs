using ShareBook.Domain.Enums;

namespace Sharebook.Jobs
{
    public interface IJob
    {
        string JobName { get; set; }
        string Description { get; set; }
        Interval Interval { get; set; }
        bool Active { get; set; }
        bool HasWork();
        bool Execute();
    }
}
