using System;
using System.Collections.Generic;

namespace ShareBook.Domain.DTOs;

public class ImporterDashboardDTO
{
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalItems { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string LastRunStatus { get; set; }
    public string LastRunMessage { get; set; }
    public IList<ImporterSourceStatusDTO> Sources { get; set; } = new List<ImporterSourceStatusDTO>();
}

public class ImporterQueueItemsPageDTO
{
    public int Page { get; set; }
    public int ItemsPerPage { get; set; }
    public int TotalItems { get; set; }
    public IList<ImporterQueueItemDTO> Items { get; set; } = new List<ImporterQueueItemDTO>();
}

public class ImporterQueueItemDTO
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public string SourceName { get; set; }
    public int Position { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string SourceUrl { get; set; }
    public string Status { get; set; }
    public string PlannedTitle { get; set; }
    public string PlannedAuthor { get; set; }
    public string PlannedCategoryId { get; set; }
    public int Attempts { get; set; }
    public string LastError { get; set; }
    public string SharebookBookId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ImporterSourceStatusDTO
{
    public int SourceId { get; set; }
    public string SourceName { get; set; }
    public string SourceUrl { get; set; }
    public bool Enabled { get; set; }
    public int TotalItems { get; set; }
    public int Done { get; set; }
    public int WaitingTriage { get; set; }
    public int Triaging { get; set; }
    public int WaitingEditor { get; set; }
    public int Editing { get; set; }
    public int WaitingProcess { get; set; }
    public int Processing { get; set; }
    public int RetryLater { get; set; }
    public int SourceBlocked { get; set; }
    public int Duplicate { get; set; }
    public int Error { get; set; }
    public string NextItemTitle { get; set; }
    public int? NextItemPosition { get; set; }
    public string NextItemStatus { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string LastRunStatus { get; set; }
    public string LastRunMessage { get; set; }
}
