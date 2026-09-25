using System;

namespace ShareBook.Api.ViewModels;

public class JobMonitorHistoryItemVM
{
    public DateTime? CreationDate { get; set; }
    public bool IsSuccess { get; set; }
    public double TimeSpentSeconds { get; set; }
    public string? Details { get; set; }
}
