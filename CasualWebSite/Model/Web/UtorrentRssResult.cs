using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Model.Web
{
    [XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
    [XmlRoot("rss")]
    public class UtorrentRssResult
    {
        public UtorrentChannel channel = new UtorrentChannel();
        
        [XmlAttribute(AttributeName = "xmlns:atom")]
        public string xmlnsAtom = "http://www.w3.org/2005/Atom";

        [XmlAttribute]
        public string version = "2.0";

        public UtorrentRssResult(IEnumerable<UtorrentRssResultItem> items)
        {
            channel.items = items.ToList();
        }

        public UtorrentRssResult()
        {

        }
    }

    [XmlRoot("channel")]
    public class UtorrentChannel
    {
        public string title = "UtorrentChannel";
        public string description = "UtorrentChannel";
        public string link = "UtorrentChannel";
        public AtomLink atomLink = new AtomLink();

        [XmlElement("item")]
        public List<UtorrentRssResultItem> items;
    }

    public class UtorrentRssResultItem
    {
        public string Title { get; set; }

        public string Link { get; set; }

        public string Guid { get; set; }

        [XmlFormat("R")]
        public DateTime PubDate { get; set; }

        [XmlCData]
        public string Description { get; set; }
    }

    [XmlMode(XmlModeAttribute.XmlModeType.NotEnclosing)]
    [XmlRoot("atom:link")]
    public class AtomLink
    {
        [XmlAttribute]
        public string href = "";

        [XmlAttribute]
        public string rel = "self";

        [XmlAttribute]
        public string type = "application/rss+xml";
    }
}
