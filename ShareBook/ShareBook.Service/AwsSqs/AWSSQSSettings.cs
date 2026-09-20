using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AwsSqs;

public class AwsSqsSettings
{
    public bool IsActive { get; set; }
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string QueueBaseUrl { get; set; } = string.Empty;
    
    // Queues
    public string NewBookQueue { get; set; } = string.Empty;

    public string SendEmailHighPriorityQueue { get; set; } = string.Empty;

    public string SendEmailLowPriorityQueue { get; set; } = string.Empty;
}
