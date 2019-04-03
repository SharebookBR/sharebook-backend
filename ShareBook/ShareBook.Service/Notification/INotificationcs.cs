using ShareBook.Domain.Enums;
using System;
using System.Threading.Tasks;
using ShareBook.Domain;
namespace ShareBook.Service.Notification
{
    public interface INotification
    {
       string SendNotificationSegments(NotificationOnesignal notficationSettings);
       string SendNotificationByUserId(NotificationOnesignal notficationSettings);
    }
}
