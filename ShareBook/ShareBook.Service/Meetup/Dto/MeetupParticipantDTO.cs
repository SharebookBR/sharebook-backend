using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Service.Dto
{
    public class MeetupParticipantDto
    {
        public List<Data> Data { get; set; }
        public Pagination Pagination { get; set; }
    }
    
    public class Data
    {
        [JsonProperty("event_id")]
        public int EventId { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class Pagination
    {
        [JsonProperty("has_next")]
        public bool HasNext { get; set; }
        [JsonProperty("hast_prev")]
        public bool HasPrev { get; set; }
        public int Quantity { get; set; }
        public int Offset { get; set; }
        public int Page { get; set; }
        [JsonProperty("page_size")]
        public int PageSize { get; set; }
        [JsonProperty("total_page")]
        public int TotalPage { get; set; }
    }

}
