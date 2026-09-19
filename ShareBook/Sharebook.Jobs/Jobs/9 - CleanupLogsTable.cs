using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using System;
using System.Threading.Tasks;

namespace Sharebook.Jobs;

public class CleanupLogsTable : GenericJob, IJob
{
    // Retenção da tabela "Logs" (eventos operacionais/segurança, ex.: rate limit de download).
    // Não confundir com "EFLogs" (auditoria de mudança de entidade), que não tem expurgo automático.
    private const int RetentionDays = 15;

    private readonly ApplicationDbContext _context;

    public CleanupLogsTable(
        IJobHistoryRepository jobHistoryRepo,
        ILoggerFactory loggerFactory,
        ApplicationDbContext context) : base(jobHistoryRepo, loggerFactory)
    {
        JobName = "CleanupLogsTable";
        Description = $"Remove da tabela Logs os eventos com mais de {RetentionDays} dias.";
        Interval = Interval.Dayly;
        Active = true;
        BestTimeToExecute = new TimeSpan(4, 0, 0);

        _context = context;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var cutoffUtc = DateTime.UtcNow.AddDays(-RetentionDays);

        var deletedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM \"Logs\" WHERE \"Timestamp\" < {cutoffUtc}");

        return new JobHistory()
        {
            JobName = JobName,
            IsSuccess = true,
            Details = $"{deletedRows} linha(s) removida(s) da tabela Logs (cutoff: {cutoffUtc:O})."
        };
    }
}
