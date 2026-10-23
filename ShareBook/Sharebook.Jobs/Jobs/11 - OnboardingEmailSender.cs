using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Repository;
using ShareBook.Service;
using ShareBook.Service.AwsSqs;
using ShareBook.Service.AwsSqs.Dto;
using ShareBook.Service.Onboarding;

namespace Sharebook.Jobs;

/// <summary>
/// Entrega a sequência fixa de ativação. A jornada só avança depois que a mensagem
/// foi aceita pela fila; repetir o job não reencaminha uma etapa já marcada como enviada.
/// </summary>
public class OnboardingEmailSender : GenericJob, IJob
{
    private static readonly TimeSpan[] DelaysAfterStep =
    [
        TimeSpan.FromDays(1),
        TimeSpan.FromDays(2),
        TimeSpan.FromDays(2),
        TimeSpan.FromDays(2)
    ];

    private readonly ApplicationDbContext _context;
    private readonly MailSenderLowPriorityQueue _mailQueue;
    private readonly IEmailTemplate _emailTemplate;
    private readonly IEmailService _emailService;
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly OnboardingSettings _settings;

    public OnboardingEmailSender(
        IJobHistoryRepository jobHistoryRepo,
        ILoggerFactory loggerFactory,
        TimeProvider timeProvider,
        ApplicationDbContext context,
        MailSenderLowPriorityQueue mailQueue,
        IEmailTemplate emailTemplate,
        IEmailService emailService,
        IUserService userService,
        IConfiguration configuration,
        IOptions<OnboardingSettings> settings) : base(jobHistoryRepo, loggerFactory, timeProvider)
    {
        JobName = "OnboardingEmailSender";
        Description = "Envia a sequência de cinco e-mails de ativação para novos cadastros elegíveis.";
        Interval = Interval.Each5Minutes;
        Active = true;

        _context = context;
        _mailQueue = mailQueue;
        _emailTemplate = emailTemplate;
        _emailService = emailService;
        _userService = userService;
        _configuration = configuration;
        _settings = settings.Value;
    }

    public override async Task<JobHistory> WorkAsync()
    {
        if (!bool.Parse(_configuration["AwsSqsSettings:IsActive"] ?? "false"))
            throw new AwsSqsDisabledException("Servico AWS SQS esta desabilitado no appsettings.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var journeys = await _context.OnboardingJourneys
            .Include(journey => journey.User)
            .Where(journey => journey.CompletedAtUtc == null
                && journey.CanceledAtUtc == null
                && journey.NextEmailAtUtc <= now)
            .OrderBy(journey => journey.NextEmailAtUtc)
            .Take(Math.Max(1, _settings.MaxJourneysPerRun))
            .ToListAsync();

        var queued = 0;
        var canceled = 0;
        foreach (var journey in journeys)
        {
            var user = journey.User;
            if (!IsEligible(user))
            {
                Cancel(journey, now, "email_not_eligible");
                canceled++;
                continue;
            }

            if (await _emailService.IsBounceAsync(user.Email))
            {
                Cancel(journey, now, "email_bounce");
                canceled++;
                continue;
            }

            var mail = await BuildMailAsync(journey.NextStep, user);
            await _mailQueue.SendMessageAsync(mail);

            journey.LastQueuedAtUtc = now;
            if (journey.NextStep == 4)
            {
                journey.NextStep = 5;
                journey.CompletedAtUtc = now;
                journey.CompletionReason = "sequence_complete";
            }
            else
            {
                journey.NextStep++;
                journey.NextEmailAtUtc = now.Add(DelaysAfterStep[journey.NextStep - 1]);
            }

            queued++;
        }

        if (journeys.Count > 0)
            await _context.SaveChangesAsync();

        return new JobHistory
        {
            JobName = JobName,
            IsSuccess = true,
            Details = $"Jornadas vencidas: {journeys.Count}. Etapas colocadas na fila: {queued}. Jornadas canceladas: {canceled}."
        };
    }

    private async Task<MailSenderbody> BuildMailAsync(int step, User user)
    {
        var frontendUrl = _configuration["ServerSettings:FrontendUrl"] ?? string.Empty;
        var backendUrl = _configuration["ServerSettings:BackendUrl"] ?? string.Empty;
        var unsubscribeToken = _userService.GenerateUnsubscribeToken(user.Id);
        var viewModel = new
        {
            Name = "{name}",
            FrontendUrl = frontendUrl,
            BackendUrl = backendUrl,
            CommunityUrl = _settings.CommunityUrl,
            UnsubscribeUrl = $"{frontendUrl}/descadastrar?userId={user.Id}&token={unsubscribeToken}"
        };

        var (template, subject) = step switch
        {
            0 => ("OnboardingWelcomeTemplate", "Bem-vindo ao Sharebook"),
            1 => ("OnboardingCatalogTemplate", "Seu próximo livro pode estar aqui"),
            2 => ("OnboardingDonationImpactTemplate", "Todo livro importa. Muito."),
            3 => ("OnboardingDonationFlowTemplate", "Não é sorteio. É escolha."),
            4 => ("OnboardingCommunityTemplate", "A comunidade Sharebook está te esperando"),
            _ => throw new InvalidOperationException($"Etapa de onboarding inválida: {step}.")
        };

        return new MailSenderbody
        {
            Subject = subject,
            BodyHTML = await _emailTemplate.GenerateHtmlFromTemplateAsync(template, viewModel),
            Destinations = [new Destination { Name = user.Name, Email = user.Email }]
        };
    }

    private static bool IsEligible(User user) => user.Active
        && user.ParentAproved
        && user.AllowSendingEmail
        && EmailAddressValidator.IsValid(user.Email);

    private static void Cancel(OnboardingJourney journey, DateTime now, string reason)
    {
        journey.CanceledAtUtc = now;
        journey.CompletionReason = reason;
    }
}
