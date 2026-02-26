using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.Notification;

public class PushNotificationService : IPushNotificationService
{
    private const string OneSignalApiUrl = "https://onesignal.com/api/v1/notifications";

    private readonly PushNotificationSettings _settings;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(IOptions<PushNotificationSettings> pushNotificationSettings, ILogger<PushNotificationService> logger)
    {
        _settings = pushNotificationSettings.Value;
        _logger = logger;
    }

    public async Task<string> SendNotificationSegmentsAsync(NotificationOnesignal onesignal)
    {
        if (!_settings.IsActive) return "";

        var payload = new
        {
            app_id = _settings.AppId,
            headings = new Dictionary<string, string> { ["pt"] = onesignal.Title },
            contents = new Dictionary<string, string> { ["pt"] = onesignal.Content },
            included_segments = new[] { GetSegments(onesignal.TypeSegments) }
        };

        await PostNotificationAsync(payload);

        return "Enviado com sucesso";
    }

    public async Task<string> SendNotificationByKeyAsync(NotificationOnesignal onesignal)
    {
        if (!_settings.IsActive) return "";

        var payload = new
        {
            app_id = _settings.AppId,
            headings = new Dictionary<string, string> { ["en"] = onesignal.Title, ["pt"] = onesignal.Title },
            contents = new Dictionary<string, string> { ["en"] = onesignal.Content, ["pt"] = onesignal.Content },
            filters = new[] { new { field = "tag", key = onesignal.Key, relation = "=", value = onesignal.Value } }
        };

        await PostNotificationAsync(payload);

        return $"Notification enviado para o {onesignal.Value} com sucesso";
    }

    public async Task<string> SendNotificationByEmailAsync(string email, string title, string content)
    {
        if (!_settings.IsActive) return "";

        try
        {
            var payload = new
            {
                app_id = _settings.AppId,
                headings = new Dictionary<string, string> { ["en"] = title, ["pt"] = title },
                contents = new Dictionary<string, string> { ["en"] = content, ["pt"] = content },
                filters = new[] { new { field = "tag", key = "email", relation = "=", value = email } }
            };

            await PostNotificationAsync(payload);

            return $"Notification enviado para o {email} com sucesso";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar push notification para {Email}", email);
            return "";
        }
    }

    private async Task PostNotificationAsync(object payload)
    {
        await OneSignalApiUrl
            .WithHeader("Authorization", $"Basic {_settings.ApiKey}")
            .PostJsonAsync(payload);
    }

    private string GetSegments(TypeSegments typeSegments)
    {
        return typeSegments switch
        {
            TypeSegments.Inactive => "Inactive Users",
            TypeSegments.Engaged => "Engaged Users",
            TypeSegments.All => "Subscribed Users",
            TypeSegments.Active => "Active Users",
            _ => ""
        };
    }
}
