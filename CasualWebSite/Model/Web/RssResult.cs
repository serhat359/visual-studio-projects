using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Model.Web
{
    [XmlRoot("rss")]
    public class RssResult
    {
        public Channel channel = new Channel();

        [XmlAttribute]
        public string version = "2.0";

        public RssResult(IEnumerable<RssResultItem> items)
        {
            channel.items = items.ToList();
        }

        public RssResult()
        {

        }
    }

    [XmlRoot("channel")]
    public class Channel
    {
        [XmlElement("item")]
        public List<RssResultItem> items;
    }

    [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
    public class RssResultItem
    {
        public string Title { get; set; }
        
        public string Link { get; set; }
        
        public string Description { get; set; }
        
        [XmlFormat("R")]
        public DateTime PubDate { get; set; }
    }
}
