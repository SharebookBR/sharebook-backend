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
    public string Title { get; set; }
    public string Author { get; set; }
    public string SourceUrl { get; set; }
    public string Status { get; set; }
    public string PlannedTitle { get; set; }
    public string PlannedAuthor { get; set; }
    public string PlannedCategoryId { get; set; }
    public string PlannedCategoryName { get; set; }
    public string PlannedCategoryParentName { get; set; }
    public int TriageAttempts { get; set; }
    public int PublishAttempts { get; set; }
    public string LastError { get; set; }
    public string SharebookBookId { get; set; }
    public string BookSlug { get; set; }
    public string BookImageSlug { get; set; }
    public string BookThumbnailUrl { get; set; }
    public string MetadataJson { get; set; }
    public string PlannedSynopsis { get; set; }
    public string PlannedCoverMode { get; set; }
    public string PlannedCoverUrl { get; set; }
    public string PlannedBy { get; set; }
    public DateTime? PlannedAt { get; set; }
    public string AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ImporterQueueItemHistoryEntryDTO
{
    public DateTime ChangedAt { get; set; }
    public string FromStatus { get; set; }
    public string ToStatus { get; set; }
}

public class ImporterSourceStatusDTO
{
    public int SourceId { get; set; }
    public string SourceName { get; set; }
    public string SourceUrl { get; set; }
    public bool Enabled { get; set; }
    public int TotalItems { get; set; }
    public int Done { get; set; }
    public int EditorialRejected { get; set; }
    public int WaitingTriage { get; set; }
    public int Triaging { get; set; }
    public int WaitingEditorial { get; set; }
    public int Editing { get; set; }
    public int WaitingPublish { get; set; }
    public int Publishing { get; set; }
    public int TriageRetry { get; set; }
    public int PublishRetry { get; set; }
    public int SourceBlocked { get; set; }
    public int Duplicate { get; set; }
    public int Error { get; set; }
    public int Unknown { get; set; }
    public int TriageRejected { get; set; }
    public string NextItemTitle { get; set; }
    public string NextItemStatus { get; set; }
    public DateTime? LastRunAt { get; set; }
    public string LastRunStatus { get; set; }
    public string LastRunMessage { get; set; }
    // D-1 counts (null = sem histórico ainda)
    public int? DoneD1 { get; set; }
    public int? EditorialRejectedD1 { get; set; }
    public int? WaitingTriageD1 { get; set; }
    public int? TriagingD1 { get; set; }
    public int? WaitingEditorialD1 { get; set; }
    public int? EditingD1 { get; set; }
    public int? WaitingPublishD1 { get; set; }
    public int? PublishingD1 { get; set; }
    public int? TriageRetryD1 { get; set; }
    public int? PublishRetryD1 { get; set; }
    public int? SourceBlockedD1 { get; set; }
    public int? DuplicateD1 { get; set; }
    public int? ErrorD1 { get; set; }
    public int? UnknownD1 { get; set; }
    public int? TriageRejectedD1 { get; set; }
}
