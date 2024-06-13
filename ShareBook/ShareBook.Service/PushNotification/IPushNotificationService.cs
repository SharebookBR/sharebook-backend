using ShareBook.Domain;
using System.Threading.Tasks;

namespace ShareBook.Service.Notification
{
    public interface IPushNotificationService
    {
        Task<string> SendNotificationSegmentsAsync(NotificationOnesignal notficationSettings);
        Task<string> SendNotificationByKeyAsync(NotificationOnesignal notficationSettings);
        Task<string> SendNotificationByEmailAsync(string email, string title, string content);
    }
}
