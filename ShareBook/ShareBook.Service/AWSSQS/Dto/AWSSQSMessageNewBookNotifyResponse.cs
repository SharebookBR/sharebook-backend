using System.Collections.Generic;

namespace ShareBook.Service.AWSSQS.Dto
{
    public class AWSSQSMessageNewBookNotifyResponse
    {
        public string ReceiptHandle { get; set; }
        public string Subject { get; set; }
        public string BodyHTML { get; set; }
        public IList<DestinationResponse> Destinations { get; set; }
    }

    public class DestinationResponse
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}