using System.Collections.Generic;

namespace ShareBook.Service.AWSSQS.Dto
{
    public class AWSSQSMessageNewBookNotifyRequest
    {
        public string Subject { get; set; }
        public string BodyHTML { get; set; }
        public IList<DestinationRequest> Destinations { get; set; }
    }

    public class DestinationRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }
}