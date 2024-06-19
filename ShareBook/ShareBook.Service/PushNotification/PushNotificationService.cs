using Microsoft.Extensions.Options;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;
using Rollbar;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.Notification;

public class PushNotificationService : IPushNotificationService
{
    private readonly PushNotificationSettings _settings;
    private readonly OneSignalClient _oneSignalClient;
    public PushNotificationService(IOptions<PushNotificationSettings> pushNotificationSettings)
    {
        _settings = pushNotificationSettings.Value;
        _oneSignalClient = new OneSignalClient(_settings.ApiKey);
    }

    public async Task<string> SendNotificationSegmentsAsync(NotificationOnesignal onesignal)
    {
        if (!_settings.IsActive) return "";

        var notificationCreateOptions = new NotificationCreateOptions
        {
            AppId = new Guid(_settings.AppId)
        };

        notificationCreateOptions.IncludedSegments = new List<string>()
        {
            GetSegments(onesignal.TypeSegments)
        };

        notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, onesignal.Title);
        notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, onesignal.Content);

        await _oneSignalClient.Notifications.CreateAsync(notificationCreateOptions);

        return "Enviado com sucesso";
    }

    public async Task<string> SendNotificationByKeyAsync(NotificationOnesignal onesignal)
    {
        if (!_settings.IsActive) return "";

        var notificationCreateOptions = new NotificationCreateOptions
        {
            AppId = new Guid(_settings.AppId)
        };

        notificationCreateOptions.Filters = new List<INotificationFilter>
        {
            new NotificationFilterField { Field = NotificationFilterFieldTypeEnum.Tag, Key = onesignal.Key, Value = onesignal.Value}
        };

        notificationCreateOptions.Headings.Add(LanguageCodes.English, onesignal.Title);
        notificationCreateOptions.Contents.Add(LanguageCodes.English, onesignal.Content);

        notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, onesignal.Title);
        notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, onesignal.Content);

        await _oneSignalClient.Notifications.CreateAsync(notificationCreateOptions);

        return $"Notification enviado para o {onesignal.Value} com sucesso";
    }


    public async Task<string> SendNotificationByEmailAsync(string email, string title, string content)
    {
        if (!_settings.IsActive) return "";

        try {
            var notificationCreateOptions = new NotificationCreateOptions
            {
                AppId = new Guid(_settings.AppId)
            };

            notificationCreateOptions.Filters = new List<INotificationFilter>
            {
                new NotificationFilterField { Field = NotificationFilterFieldTypeEnum.Tag, Key = "email", Value = email}
            };

            notificationCreateOptions.Headings.Add(LanguageCodes.English, title);
            notificationCreateOptions.Contents.Add(LanguageCodes.English, content);

            notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, title);
            notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, content);

            await _oneSignalClient.Notifications.CreateAsync(notificationCreateOptions);

            return $"Notification enviado para o {email} com sucesso";
        }
        catch(Exception ex) {
            RollbarLocator.RollbarInstance.Error(ex);
            return "";
        }            
    }

    private string GetSegments(TypeSegments typeSegments)
    {
        switch (typeSegments)
        {
            case TypeSegments.Inactive:
                return "Inactive Users";
            case TypeSegments.Engaged:
                return "Engaged Users";
            case TypeSegments.All:
                return "Subscribed Users";
            case TypeSegments.Active:
                return "Active Users";
            default:
                return "";
        }
    }
}
