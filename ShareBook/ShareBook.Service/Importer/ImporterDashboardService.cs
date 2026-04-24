using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using ShareBook.Domain.DTOs;

namespace ShareBook.Service.Importer;

public class ImporterDashboardService : IImporterDashboardService
{
    private readonly IConfiguration _configuration;

    public ImporterDashboardService(IConfiguration configuration)
    {
        _configuration = configuration;
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
last_runs AS (
    SELECT DISTINCT ON (1)
        started_at,
        status,
        message
    FROM importer.runs
    ORDER BY started_at DESC
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
    lr.started_at AS last_run_at,
    lr.status AS last_run_status,
    lr.message AS last_run_message
FROM source_status ss
LEFT JOIN next_items ni ON ni.source_id = ss.source_id
LEFT JOIN last_runs lr ON TRUE
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
}
