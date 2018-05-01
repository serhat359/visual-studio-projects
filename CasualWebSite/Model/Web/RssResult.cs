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

    public class RssResultItem
    {
        [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
        public string Title { get; set; }

        [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
        public string Link { get; set; }

        [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
        public string Description { get; set; }

        [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
        [XmlFormat("R")]
        public DateTime PubDate { get; set; }
    }
}
