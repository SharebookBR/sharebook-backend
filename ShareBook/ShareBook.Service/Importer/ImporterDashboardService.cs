using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ShareBook.Domain.DTOs;
using ShareBook.Repository;

namespace ShareBook.Service.Importer;

public class ImporterDashboardService : IImporterDashboardService
{
    private readonly IConfiguration _configuration;
    private readonly IBookRepository _bookRepository;

    private static readonly ISet<string> ValidStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "waiting_triage",
        "triaging",
        "triage_rejected",
        "waiting_editor",
        "editing",
        "waiting_process",
        "processing",
        "done",
        "retry_later",
        "source_blocked",
        "duplicate",
        "error"
    };

    public ImporterDashboardService(IConfiguration configuration, IBookRepository bookRepository)
    {
        _configuration = configuration;
        _bookRepository = bookRepository;
    }

    public async Task<ImporterDashboardDTO> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("ImporterPostgresConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:ImporterPostgresConnection não configurada.");

        const string sql = @"
WITH source_status AS (
    SELECT
        s.id AS source_id,
        s.name AS source_name,
        s.url AS source_url,
        s.enabled,
        COUNT(q.id) AS total_items,
        COUNT(*) FILTER (WHERE q.status = 'done') AS done,
        COUNT(*) FILTER (WHERE q.status = 'waiting_triage') AS waiting_triage,
        COUNT(*) FILTER (WHERE q.status = 'triaging') AS triaging,
        COUNT(*) FILTER (WHERE q.status = 'triage_rejected') AS triage_rejected,
        COUNT(*) FILTER (WHERE q.status = 'waiting_editor') AS waiting_editor,
        COUNT(*) FILTER (WHERE q.status = 'editing') AS editing,
        COUNT(*) FILTER (WHERE q.status = 'waiting_process') AS waiting_process,
        COUNT(*) FILTER (WHERE q.status = 'processing') AS processing,
        COUNT(*) FILTER (WHERE q.status = 'retry_later') AS retry_later,
        COUNT(*) FILTER (WHERE q.status = 'source_blocked') AS source_blocked,
        COUNT(*) FILTER (WHERE q.status = 'duplicate') AS duplicate,
        COUNT(*) FILTER (WHERE q.status = 'error') AS error
    FROM importer.sources s
    LEFT JOIN importer.queue_items q ON q.source_id = s.id
    GROUP BY s.id, s.name, s.url, s.enabled
),
next_items AS (
    SELECT DISTINCT ON (source_id)
        source_id,
        position,
        title,
        status
    FROM importer.queue_items
    WHERE status IN ('waiting_triage', 'triaging', 'waiting_editor', 'editing', 'waiting_process', 'processing', 'retry_later', 'error')
    ORDER BY source_id, position ASC
),
global_last_run AS (
    SELECT
        started_at,
        status,
        message
    FROM importer.runs
    ORDER BY started_at DESC, id DESC
    LIMIT 1
),
source_last_runs AS (
    SELECT DISTINCT ON (qi.source_id)
        qi.source_id,
        r.started_at,
        r.status,
        r.message
    FROM importer.runs r
    JOIN importer.queue_items qi ON qi.id = r.processed_item_id
    WHERE qi.source_id IS NOT NULL
    ORDER BY qi.source_id, r.started_at DESC, r.id DESC
)
SELECT
    ss.source_id,
    ss.source_name,
    ss.source_url,
    ss.enabled,
    ss.total_items,
    ss.done,
    ss.waiting_triage,
    ss.triaging,
    ss.triage_rejected,
    ss.waiting_editor,
    ss.editing,
    ss.waiting_process,
    ss.processing,
    ss.retry_later,
    ss.source_blocked,
    ss.duplicate,
    ss.error,
    ni.position AS next_item_position,
    ni.title AS next_item_title,
    ni.status AS next_item_status,
    slr.started_at AS last_run_at,
    slr.status AS last_run_status,
    slr.message AS last_run_message,
    glr.started_at AS global_last_run_at,
    glr.status AS global_last_run_status,
    glr.message AS global_last_run_message
FROM source_status ss
LEFT JOIN next_items ni ON ni.source_id = ss.source_id
LEFT JOIN source_last_runs slr ON slr.source_id = ss.source_id
LEFT JOIN global_last_run glr ON TRUE
ORDER BY ss.source_id;
";

        var result = new ImporterDashboardDTO
        {
            GeneratedAtUtc = DateTime.UtcNow
        };

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            if (result.LastRunAt is null)
            {
                result.LastRunAt = reader.IsDBNull(reader.GetOrdinal("global_last_run_at")) ? null : reader.GetDateTime(reader.GetOrdinal("global_last_run_at"));
                result.LastRunStatus = reader.IsDBNull(reader.GetOrdinal("global_last_run_status")) ? null : reader.GetString(reader.GetOrdinal("global_last_run_status"));
                result.LastRunMessage = reader.IsDBNull(reader.GetOrdinal("global_last_run_message")) ? null : reader.GetString(reader.GetOrdinal("global_last_run_message"));
            }

            var item = new ImporterSourceStatusDTO
            {
                SourceId = reader.GetInt32(reader.GetOrdinal("source_id")),
                SourceName = reader.GetString(reader.GetOrdinal("source_name")),
                SourceUrl = reader.GetString(reader.GetOrdinal("source_url")),
                Enabled = reader.GetBoolean(reader.GetOrdinal("enabled")),
                TotalItems = reader.GetInt32(reader.GetOrdinal("total_items")),
                Done = reader.GetInt32(reader.GetOrdinal("done")),
                WaitingTriage = reader.GetInt32(reader.GetOrdinal("waiting_triage")),
                Triaging = reader.GetInt32(reader.GetOrdinal("triaging")),
                TriageRejected = reader.GetInt32(reader.GetOrdinal("triage_rejected")),
                WaitingEditor = reader.GetInt32(reader.GetOrdinal("waiting_editor")),
                Editing = reader.GetInt32(reader.GetOrdinal("editing")),
                WaitingProcess = reader.GetInt32(reader.GetOrdinal("waiting_process")),
                Processing = reader.GetInt32(reader.GetOrdinal("processing")),
                RetryLater = reader.GetInt32(reader.GetOrdinal("retry_later")),
                SourceBlocked = reader.GetInt32(reader.GetOrdinal("source_blocked")),
                Duplicate = reader.GetInt32(reader.GetOrdinal("duplicate")),
                Error = reader.GetInt32(reader.GetOrdinal("error")),
                NextItemPosition = reader.IsDBNull(reader.GetOrdinal("next_item_position")) ? null : reader.GetInt32(reader.GetOrdinal("next_item_position")),
                NextItemTitle = reader.IsDBNull(reader.GetOrdinal("next_item_title")) ? null : reader.GetString(reader.GetOrdinal("next_item_title")),
                NextItemStatus = reader.IsDBNull(reader.GetOrdinal("next_item_status")) ? null : reader.GetString(reader.GetOrdinal("next_item_status")),
                LastRunAt = reader.IsDBNull(reader.GetOrdinal("last_run_at")) ? null : reader.GetDateTime(reader.GetOrdinal("last_run_at")),
                LastRunStatus = reader.IsDBNull(reader.GetOrdinal("last_run_status")) ? null : reader.GetString(reader.GetOrdinal("last_run_status")),
                LastRunMessage = reader.IsDBNull(reader.GetOrdinal("last_run_message")) ? null : reader.GetString(reader.GetOrdinal("last_run_message")),
            };

            result.TotalItems += item.TotalItems;
            result.Sources.Add(item);
        }

        return result;
    }

    public async Task<ImporterQueueItemsPageDTO> GetItemsAsync(int? sourceId, string status, int? position, string sort, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("ImporterPostgresConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:ImporterPostgresConnection não configurada.");

        var normalizedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim().ToLowerInvariant();
        if (normalizedStatus is not null && !ValidStatuses.Contains(normalizedStatus))
            throw new ArgumentException("Status inválido para fila do importador.", nameof(status));

        var orderBySql = sort?.ToLowerInvariant() switch
        {
            "position_asc" => "ORDER BY q.position ASC, q.id ASC",
            _ => "ORDER BY q.updated_at DESC, q.id DESC"
        };

        var safePage = Math.Max(page, 1);
        var safePageSize = Math.Min(Math.Max(pageSize, 1), 200);
        var offset = (safePage - 1) * safePageSize;
        var where = new List<string>();

        if (sourceId.HasValue)
            where.Add("q.source_id = @source_id");

        if (normalizedStatus is not null)
            where.Add("q.status = @status");

        if (position.HasValue)
            where.Add("q.position = @position");

        var whereSql = where.Count > 0 ? $"WHERE {string.Join(" AND ", where)}" : string.Empty;

        var countSql = $@"
SELECT COUNT(*)
FROM importer.queue_items q
JOIN importer.sources s ON s.id = q.source_id
{whereSql};
";

        var itemsSql = $@"
SELECT
    q.id,
    q.source_id,
    s.name AS source_name,
    q.position,
    q.title,
    q.author,
    q.source_url,
    q.status,
    q.planned_title,
    q.planned_author,
    q.planned_category_id,
    q.attempts,
    q.last_error,
    q.sharebook_book_id,
    q.metadata_json,
    q.created_at,
    q.updated_at
FROM importer.queue_items q
JOIN importer.sources s ON s.id = q.source_id
{whereSql}
{orderBySql}
LIMIT @limit OFFSET @offset;
";

        var result = new ImporterQueueItemsPageDTO
        {
            Page = safePage,
            ItemsPerPage = safePageSize
        };

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync(cancellationToken);

        await using (var countCmd = new NpgsqlCommand(countSql, conn))
        {
            AddItemFilterParameters(countCmd, sourceId, normalizedStatus, position);
            result.TotalItems = Convert.ToInt32(await countCmd.ExecuteScalarAsync(cancellationToken));
        }

        await using (var itemsCmd = new NpgsqlCommand(itemsSql, conn))
        {
            AddItemFilterParameters(itemsCmd, sourceId, normalizedStatus, position);
            itemsCmd.Parameters.AddWithValue("limit", safePageSize);
            itemsCmd.Parameters.AddWithValue("offset", offset);

            await using var reader = await itemsCmd.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Items.Add(new ImporterQueueItemDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    SourceId = reader.GetInt32(reader.GetOrdinal("source_id")),
                    SourceName = reader.GetString(reader.GetOrdinal("source_name")),
                    Position = reader.GetInt32(reader.GetOrdinal("position")),
                    Title = reader.GetString(reader.GetOrdinal("title")),
                    Author = GetUniversalString(reader, "author"),
                    SourceUrl = reader.GetString(reader.GetOrdinal("source_url")),
                    Status = reader.GetString(reader.GetOrdinal("status")),
                    PlannedTitle = GetUniversalString(reader, "planned_title"),
                    PlannedAuthor = GetUniversalString(reader, "planned_author"),
                    PlannedCategoryId = GetUniversalString(reader, "planned_category_id"),
                    Attempts = reader.GetInt32(reader.GetOrdinal("attempts")),
                    LastError = GetUniversalString(reader, "last_error"),
                    SharebookBookId = GetUniversalString(reader, "sharebook_book_id"),
                    MetadataJson = GetUniversalString(reader, "metadata_json"),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                });
            }
        }

        await EnrichWithBookSlugsAsync(result.Items);

        return result;
    }

    private async Task EnrichWithBookSlugsAsync(IList<ImporterQueueItemDTO> items)
    {
        var bookIds = items
            .Where(x => !string.IsNullOrWhiteSpace(x.SharebookBookId))
            .Select(x => Guid.Parse(x.SharebookBookId))
            .Distinct()
            .ToList();

        if (!bookIds.Any()) return;

        var books = await _bookRepository.GetAsync(x => bookIds.Contains(x.Id));
        var slugMap = books.ToDictionary(x => x.Id, x => x.Slug);

        foreach (var item in items)
        {
            if (!string.IsNullOrWhiteSpace(item.SharebookBookId) &&
                Guid.TryParse(item.SharebookBookId, out var bookId) &&
                slugMap.TryGetValue(bookId, out var slug))
            {
                item.BookSlug = slug;
            }
        }
    }

    private static void AddItemFilterParameters(NpgsqlCommand command, int? sourceId, string status, int? position)
    {
        if (sourceId.HasValue)
            command.Parameters.AddWithValue("source_id", sourceId.Value);

        if (status is not null)
            command.Parameters.AddWithValue("status", status);

        if (position.HasValue)
            command.Parameters.AddWithValue("position", position.Value);
    }

    private static string GetUniversalString(NpgsqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        if (reader.IsDBNull(ordinal)) return null;

        var value = reader.GetValue(ordinal);
        return value?.ToString();
    }

    private static string GetNullableString(NpgsqlDataReader reader, string columnName)
    {
        return GetUniversalString(reader, columnName);
    }
}
