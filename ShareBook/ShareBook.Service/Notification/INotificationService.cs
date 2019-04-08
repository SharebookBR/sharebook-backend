using ShareBook.Domain;

namespace ShareBook.Service.Notification
{
    public interface INotificationService
    {
        string SendNotificationSegments(NotificationOnesignal notficationSettings);
        string SendNotificationByKey(NotificationOnesignal notficationSettings);
        string SendNotificationByEmail(string email, string title, string content);
    }
}
