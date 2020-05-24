using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AWSSQS
{
    public class AWSSQSSettings
    {
        public bool IsActive { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string QueueUrl { get; set; }
    }
}
