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

namespace ShareBook.Service.Notification
{
    public class PushNotificationService : IPushNotificationService
    {
        private readonly PushNotificationSettings _settings;
        private readonly OneSignalClient _oneSignalClient;
        private readonly NotificationCreateOptions _notificationCreateOptions;
        public PushNotificationService(IOptions<PushNotificationSettings> pushNotificationSettings)
        {
            _settings = pushNotificationSettings.Value;
            _oneSignalClient = new OneSignalClient(_settings.ApiKey);
            _notificationCreateOptions = new NotificationCreateOptions
            {
                AppId = new Guid(_settings.AppId)
            };
        }

        public string SendNotificationSegments(NotificationOnesignal onesignal)
        {
            // TODO: verificar se esse serviço está ativo no appsettings.

            _notificationCreateOptions.IncludedSegments = new List<string>()
            {
                GetSegments(onesignal.TypeSegments)
            };

            _notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, onesignal.Title);
            _notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, onesignal.Content);

            _oneSignalClient.Notifications.Create(_notificationCreateOptions);

            return "Enviado com sucesso";
        }

        public string SendNotificationByKey(NotificationOnesignal onesignal)
        {
            // TODO: verificar se esse serviço está ativo no appsettings.

            _notificationCreateOptions.Filters = new List<INotificationFilter>
            {
                new NotificationFilterField { Field = NotificationFilterFieldTypeEnum.Tag, Key = onesignal.Key, Value = onesignal.Value}
            };

            _notificationCreateOptions.Headings.Add(LanguageCodes.English, onesignal.Title);
            _notificationCreateOptions.Contents.Add(LanguageCodes.English, onesignal.Content);

            _notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, onesignal.Title);
            _notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, onesignal.Content);

            _oneSignalClient.Notifications.Create(_notificationCreateOptions);


            return $"Notification enviado para o {onesignal.Value} com sucesso";
        }


        public string SendNotificationByEmail(string email, string title, string content)
        {
            if (!_settings.IsActive) return "";

            try {
                _notificationCreateOptions.Filters = new List<INotificationFilter>
                {
                        new NotificationFilterField { Field = NotificationFilterFieldTypeEnum.Tag, Key = "email", Value = email}
                };

                _notificationCreateOptions.Headings.Add(LanguageCodes.English, title);
                _notificationCreateOptions.Contents.Add(LanguageCodes.English, content);

                _notificationCreateOptions.Headings.Add(LanguageCodes.Portuguese, title);
                _notificationCreateOptions.Contents.Add(LanguageCodes.Portuguese, content);

                _oneSignalClient.Notifications.Create(_notificationCreateOptions);

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
}
