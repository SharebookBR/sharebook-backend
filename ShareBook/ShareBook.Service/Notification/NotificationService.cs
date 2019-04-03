using Microsoft.Extensions.Options;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources;
using OneSignal.RestAPIv3.Client.Resources.Notifications;
using ShareBook.Domain;
using ShareBook.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareBook.Service.Notification
{
    public class NotificationService : INotification
    {
        private readonly NotificationSettings _settings;
        private readonly OneSignalClient _oneSignalClient;

        public NotificationService(IOptions<NotificationSettings> notificationSettings)
        {
            _settings = notificationSettings.Value;
            _oneSignalClient =  new OneSignalClient(_settings.ApiKey);
        }
       
        public string SendNotificationSegments(NotificationOnesignal notficationSettings)
        {
            try
            {
                var options = new NotificationCreateOptions
                {
                    AppId = new Guid(_settings.AppId),
                    IncludedSegments = new List<string>()
                    {
                        GetSegments(notficationSettings.TypeSegments)
                    },
                    
                };

                options.Headings.Add(LanguageCodes.Portuguese, notficationSettings.Title);
                options.Contents.Add(LanguageCodes.Portuguese, notficationSettings.Content);

                _oneSignalClient.Notifications.Create(options);

            }
            catch (Exception ex)
            {
                new Exception($"Error executing SendNotificationSegments. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
            }

            return "Enviado com sucesso";
        }

        public string SendNotificationByUserId(NotificationOnesignal notficationSettings)
        {
            try
            {
                var options = new NotificationCreateOptions
                {
                    AppId = new Guid(_settings.AppId),

                    IncludePlayerIds = new List<string>()
                    {
                         notficationSettings.PlayerId
                    }
                };

                options.Headings.Add(LanguageCodes.English, notficationSettings.Title);
                options.Contents.Add(LanguageCodes.English, notficationSettings.Content);

                options.Headings.Add(LanguageCodes.Portuguese, notficationSettings.Title);
                options.Contents.Add(LanguageCodes.Portuguese, notficationSettings.Content);

              
                _oneSignalClient.Notifications.Create(options);

            }
            catch (Exception ex)
            {
                new Exception($"Error executing SendNotificationByUserId. Exception: {ex.Message}. StackTrace: {ex.StackTrace}");
            }

            return $"Notification enviado para o {notficationSettings.PlayerId} com sucesso";
        }

        private string GetSegments(TypeSegments typeSegments)
        {
            switch(typeSegments)
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
}
