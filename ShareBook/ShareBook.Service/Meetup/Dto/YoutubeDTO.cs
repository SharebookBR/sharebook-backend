using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareBook.Service.Dto
{
    public class YoutubeDto
    {
        public string Message { get; set; }
        public List<Item> Items { get; set; }
    }
    public class PageInfo
    {
        public int TotalResults { get; set; }
        public int ResultsPerPage { get; set; }
    }

    public class Id
    {
        public string Kind { get; set; }
        public string VideoId { get; set; }
    }

    public class Snippet
    {
        public DateTime PublishedAt { get; set; }
        public string ChannelId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ChannelTitle { get; set; }
        public string LiveBroadcastContent { get; set; }
        public DateTime PublishTime { get; set; }
    }

    public class Item
    {
        public Id Id { get; set; }
        public Snippet Snippet { get; set; }
    }
}
