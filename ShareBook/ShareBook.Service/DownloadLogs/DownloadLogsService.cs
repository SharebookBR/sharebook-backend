using Microsoft.EntityFrameworkCore;
using ShareBook.Helper;
using ShareBook.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShareBook.Service.DownloadLogs;

public class DownloadLogsService : IDownloadLogsService
{
    // Mesma categoria emitida pelo BookController/ThrottleFilter ao logar rate limit de download.
    // Filtrar por ela é obrigatório: "Logs" é genérica, outras categorias podem aparecer no futuro.
    private const string Category = "EBookDownload.RateLimit";

    private readonly ApplicationDbContext _ctx;

    public DownloadLogsService(ApplicationDbContext context)
    {
        _ctx = context;
    }

    public async Task<IList<DownloadLogsSummaryDto>> GetSummaryAsync(DateTime from, DateTime to)
    {
        var (fromUtc, toUtcExclusive) = ToUtcRange(from, to);

        const string sql = @"
            SELECT d::date AS ""Day"",
                   COALESCE(a.allowed, 0) AS ""Allowed"",
                   COALESCE(a.blocked_throttle, 0) AS ""BlockedThrottle"",
                   COALESCE(a.blocked_daily_limit, 0) AS ""BlockedDailyLimit""
            FROM generate_series({0}::date, {1}::date, interval '1 day') d
            LEFT JOIN (
                SELECT (""Timestamp"" AT TIME ZONE 'America/Sao_Paulo')::date AS day,
                       count(*) FILTER (WHERE ""Properties""->>'Outcome' = 'Allowed') AS allowed,
                       count(*) FILTER (WHERE ""Properties""->>'Outcome' = 'BlockedThrottle') AS blocked_throttle,
                       count(*) FILTER (WHERE ""Properties""->>'Outcome' = 'BlockedDailyLimit') AS blocked_daily_limit
                FROM ""Logs""
                WHERE ""Properties""->>'LogsCategory' = {2}
                  AND ""Timestamp"" >= {3} AND ""Timestamp"" < {4}
                GROUP BY 1
            ) a ON a.day = d::date
            ORDER BY d;";

        return await _ctx.Database
            .SqlQueryRaw<DownloadLogsSummaryDto>(sql, from.Date, to.Date, Category, fromUtc, toUtcExclusive)
            .ToListAsync();
    }

    public async Task<PagedDownloadLogEventsDto> GetEventsAsync(DateTime from, DateTime to, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 1000);
        var offset = (page - 1) * pageSize;

        var (fromUtc, toUtcExclusive) = ToUtcRange(from, to);

        const string countSql = @"
            SELECT count(*) AS ""Value""
            FROM ""Logs""
            WHERE ""Properties""->>'LogsCategory' = {0}
              AND ""Timestamp"" >= {1} AND ""Timestamp"" < {2};";

        var totalItems = await _ctx.Database
            .SqlQueryRaw<int>(countSql, Category, fromUtc, toUtcExclusive)
            .SingleAsync();

        const string eventsSql = @"
            SELECT l.""Timestamp"" AS ""Timestamp"",
                   l.""Properties""->>'Ip' AS ""Ip"",
                   l.""Properties""->>'Outcome' AS ""Outcome"",
                   l.""Properties""->>'Slug' AS ""Slug"",
                   b.""Title"" AS ""Title""
            FROM ""Logs"" l
            LEFT JOIN ""Books"" b ON b.""Slug"" = l.""Properties""->>'Slug'
            WHERE l.""Properties""->>'LogsCategory' = {0}
              AND l.""Timestamp"" >= {1} AND l.""Timestamp"" < {2}
            ORDER BY l.""Timestamp"" DESC
            LIMIT {3} OFFSET {4};";

        var items = await _ctx.Database
            .SqlQueryRaw<DownloadLogEventDto>(eventsSql, Category, fromUtc, toUtcExclusive, pageSize, offset)
            .ToListAsync();

        return new PagedDownloadLogEventsDto
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    // "from"/"to" chegam como datas de calendário (São Paulo, é como o admin pensa o filtro).
    // Convertidas para o instante UTC real de início/fim do dia antes de filtrar "Timestamp".
    private static (DateTime fromUtc, DateTime toUtcExclusive) ToUtcRange(DateTime from, DateTime to)
    {
        var fromUtc = DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(from.Date);
        var toUtcExclusive = DateTimeHelper.ConvertDateTimeToUtcFromSaoPaulo(to.Date.AddDays(1));
        return (fromUtc, toUtcExclusive);
    }
}
