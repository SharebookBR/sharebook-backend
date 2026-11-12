using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.Onboarding;

namespace Sharebook.Jobs;

/// <summary>
/// Inclui as contas anteriores ao lançamento do onboarding em lotes semanais.
/// O cursor é a última conta examinada, ordenada por CreationDate desc e Id desc.
/// </summary>
public class LegacyOnboardingBackfill : GenericJob, IJob
{
    private const string JobNameValue = "LegacyOnboardingBackfill";
    private const int PageSize = 250;

    private readonly ApplicationDbContext _context;
    private readonly LegacyOnboardingBackfillSettings _settings;

    public LegacyOnboardingBackfill(
        IJobHistoryRepository jobHistoryRepo,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        ApplicationDbContext context,
        IOptions<LegacyOnboardingBackfillSettings> settings) : base(jobHistoryRepo, loggerFactory, timeProvider)
    {
        JobName = JobNameValue;
        Description = "Inclui até 2.000 contas anteriores ao onboarding, do cadastro mais novo ao mais antigo.";
        Interval = Interval.Weekly;
        Active = true;
        BestDayOfWeek = DayOfWeek.Thursday;
        BestTimeToExecute = TimeSpan.Zero;

        _context = context;
        _settings = settings.Value;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var batchSize = Math.Max(1, _settings.BatchSize);
        var cursor = await GetCursorAsync();
        var enrolled = new List<User>();
        var lastExamined = cursor;
        var exhausted = false;

        while (enrolled.Count < batchSize && !exhausted)
        {
            var candidates = await EligibleUsersAfterCursor(lastExamined)
                .OrderByDescending(user => user.CreationDate)
                .ThenByDescending(user => user.Id)
                .Take(PageSize)
                .ToListAsync();

            if (candidates.Count == 0)
            {
                exhausted = true;
                break;
            }

            foreach (var candidate in candidates)
            {
                lastExamined = Cursor.From(candidate);
                if (!EmailAddressValidator.IsValid(candidate.Email))
                    continue;

                enrolled.Add(candidate);
                if (enrolled.Count == batchSize)
                    break;
            }
        }

        if (enrolled.Count > 0)
        {
            _context.OnboardingJourneys.AddRange(enrolled.Select(user => new OnboardingJourney
            {
                UserId = user.Id,
                NextStep = 0,
                NextEmailAtUtc = now
            }));
            await _context.SaveChangesAsync();
        }

        var newest = enrolled.FirstOrDefault()?.CreationDate;
        var oldest = enrolled.LastOrDefault()?.CreationDate;
        var details = enrolled.Count == 0
            ? "Nenhuma conta elegível restante para o onboarding legado."
            : $"Lote legado criado: {enrolled.Count} jornada(s). Janela de cadastro: {newest:yyyy-MM-dd HH:mm:ss} UTC até {oldest:yyyy-MM-dd HH:mm:ss} UTC. Cursor salvo para o próximo lote.";

        Logger.LogInformation("{JobName}. {Details}", JobName, details);
        return new JobHistory
        {
            JobName = JobName,
            IsSuccess = true,
            Details = details,
            LastResult = lastExamined?.Serialize()
        };
    }

    private IQueryable<User> EligibleUsersAfterCursor(Cursor? cursor)
    {
        var query = _context.Users
            .AsNoTracking()
            .Where(user => user.Active
                && user.ParentAproved
                && user.AllowSendingEmail
                && user.CreationDate.HasValue
                && user.CreationDate < _settings.EligibleBeforeUtc
                && !_context.OnboardingJourneys.Any(journey => journey.UserId == user.Id)
                && !_context.MailBounces.Any(bounce => bounce.Email == user.Email && !bounce.IsSoft));

        if (cursor is null)
            return query;

        return query.Where(user => user.CreationDate < cursor.CreationDate
            || (user.CreationDate == cursor.CreationDate && user.Id.CompareTo(cursor.UserId) < 0));
    }

    private async Task<Cursor?> GetCursorAsync()
    {
        var lastResult = await _jobHistoryRepo.Get()
            .AsNoTracking()
            .Where(history => history.JobName == JobNameValue && history.IsSuccess && history.LastResult != null)
            .OrderByDescending(history => history.CreationDate)
            .Select(history => history.LastResult)
            .FirstOrDefaultAsync();

        return Cursor.TryParse(lastResult, out var cursor) ? cursor : null;
    }

    private sealed record Cursor(DateTime CreationDate, Guid UserId)
    {
        public static Cursor From(User user) => new(user.CreationDate!.Value, user.Id);

        public string Serialize() => $"{CreationDate:O}|{UserId:D}";

        public static bool TryParse(string? value, out Cursor? cursor)
        {
            cursor = null;
            var parts = value?.Split('|');
            if (parts?.Length != 2
                || !DateTime.TryParse(parts[0], null, System.Globalization.DateTimeStyles.RoundtripKind, out var creationDate)
                || !Guid.TryParse(parts[1], out var userId))
                return false;

            cursor = new Cursor(creationDate, userId);
            return true;
        }
    }
}
