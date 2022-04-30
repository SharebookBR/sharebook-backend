using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft;

namespace ShareBook.Service.Dto
{
    public class SymplaDTO
    {
        public string Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public List<SymplaEvent> Data { get; set; }
    }
    public class SymplaEvent
    {
        public int Id { get; set; }
        [JsonProperty("start_date")]
        public string StartDate { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
    }
}
