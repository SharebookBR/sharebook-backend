using System;
using System.Collections.Generic;
using System.Text;

namespace ShareBook.Service.AWSSQS
{
    public class AWSSQSSettings
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string QueueUrl { get; set; }
    }
}
