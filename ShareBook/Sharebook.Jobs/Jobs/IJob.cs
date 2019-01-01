using System;
namespace Sharebook.Jobs
{
    public interface IJob
    {
        string Name { get; }
        bool HasWork();
        bool Execute();
    }
}
