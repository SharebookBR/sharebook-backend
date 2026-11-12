using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Repository;
using ShareBook.Service.Server;
using System.Security.Cryptography;

namespace ShareBook.Service.Onboarding;

public class OnboardingEmailTrackingService(
    ApplicationDbContext context,
    IConfiguration configuration,
    IOptions<ServerSettings> serverSettings,
    IOptions<OnboardingSettings> onboardingSettings,
    TimeProvider timeProvider) : IOnboardingEmailTrackingService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IConfiguration _configuration = configuration;
    private readonly ServerSettings _serverSettings = serverSettings.Value;
    private readonly OnboardingSettings _onboardingSettings = onboardingSettings.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public string BuildOpenUrl(Guid deliveryId) => $"{BackendUrl}/o/open/{CreateToken(deliveryId)}";

    public string BuildClickUrl(Guid deliveryId, string cta) => $"{BackendUrl}/o/click/{CreateToken(deliveryId)}/{cta}";

    public async Task RecordOpenAsync(string token, CancellationToken cancellationToken = default)
    {
        var delivery = await FindDeliveryAsync(token, cancellationToken);
        if (delivery == null || delivery.FirstOpenedAtUtc.HasValue)
            return;

        delivery.FirstOpenedAtUtc = _timeProvider.GetUtcNow().UtcDateTime;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> RecordClickAndGetDestinationAsync(string token, string cta, CancellationToken cancellationToken = default)
    {
        var delivery = await FindDeliveryAsync(token, cancellationToken);
        if (delivery == null || !IsExpectedCta(delivery.Step, cta))
            return null;

        if (!await _context.OnboardingEmailClicks.AnyAsync(
                click => click.OnboardingEmailDeliveryId == delivery.Id && click.Cta == cta,
                cancellationToken))
        {
            _context.OnboardingEmailClicks.Add(new OnboardingEmailClick
            {
                OnboardingEmailDeliveryId = delivery.Id,
                Cta = cta,
                FirstClickedAtUtc = _timeProvider.GetUtcNow().UtcDateTime
            });
            await _context.SaveChangesAsync(cancellationToken);
        }

        return DestinationFor(delivery.Step);
    }

    private async Task<OnboardingEmailDelivery?> FindDeliveryAsync(string token, CancellationToken cancellationToken)
    {
        if (!TryReadToken(token, out var deliveryId))
            return null;

        return await _context.OnboardingEmailDeliveries
            .SingleOrDefaultAsync(delivery => delivery.Id == deliveryId, cancellationToken);
    }

    private string CreateToken(Guid deliveryId)
    {
        var payload = deliveryId.ToByteArray();
        var signature = Sign(payload);
        var value = new byte[payload.Length + signature.Length];
        Buffer.BlockCopy(payload, 0, value, 0, payload.Length);
        Buffer.BlockCopy(signature, 0, value, payload.Length, signature.Length);
        return Convert.ToBase64String(value).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private bool TryReadToken(string token, out Guid deliveryId)
    {
        deliveryId = default;
        try
        {
            var base64 = token.Replace('-', '+').Replace('_', '/');
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            var value = Convert.FromBase64String(base64);
            const int payloadLength = 16;
            const int signatureLength = 32;
            if (value.Length != payloadLength + signatureLength)
                return false;

            var payload = value[..payloadLength];
            var signature = value[payloadLength..];
            if (!CryptographicOperations.FixedTimeEquals(signature, Sign(payload)))
                return false;

            deliveryId = new Guid(payload);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private byte[] Sign(byte[] payload)
    {
        var secret = _configuration["ServerSettings:OnboardingTrackingSecret"];
        if (string.IsNullOrWhiteSpace(secret))
            secret = _configuration["ServerSettings:UnsubscribeSecret"];
        if (string.IsNullOrWhiteSpace(secret))
            secret = "dev-onboarding-tracking-secret";
        using var hmac = new HMACSHA256(System.Text.Encoding.UTF8.GetBytes(secret));
        return hmac.ComputeHash(payload);
    }

    private string BackendUrl => _serverSettings.BackendUrl.TrimEnd('/');

    private string DestinationFor(int step) => step switch
    {
        0 or 1 or 3 => $"{_serverSettings.FrontendUrl.TrimEnd('/')}/livros",
        2 => $"{_serverSettings.FrontendUrl.TrimEnd('/')}/livros/doar",
        4 when !string.IsNullOrWhiteSpace(_onboardingSettings.CommunityUrl) => _onboardingSettings.CommunityUrl,
        4 => _serverSettings.FrontendUrl,
        _ => _serverSettings.FrontendUrl
    };

    private static bool IsExpectedCta(int step, string cta) => (step, cta) switch
    {
        (0 or 1 or 3, "catalog") => true,
        (2, "donate") => true,
        (4, "community") => true,
        _ => false
    };
}
