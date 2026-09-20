using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.Notification;

public class PushNotificationSettings
{
    public bool IsActive { get; set; }
    public string AppId { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
