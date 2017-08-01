using System;
using System.Collections.Generic;

namespace LocalWebServerMVC.Models
{
    public class YoutubeSearchResult
    {
        public string kind { get; set; }

        public string etag { get; set; }

        public List<YoutubeSearchResultItem> items { get; set; }
    }

    public class YoutubeSearchResultItem
    {
        public string kind { get; set; }

        public string etag { get; set; }

        public YoutubeSearchResultId id { get; set; }

        public YoutubeSearchResultSnippet snippet { get; set; }
    }

    public class YoutubeSearchResultId
    {
        public string kind { get; set; }

        public string videoId { get; set; }
    }

    public class YoutubeSearchResultSnippet
    {
        public DateTime publishedAt { get; set; }

        public string channelId { get; set; }

        public string title { get; set; }

        public string description { get; set; }
    }
}