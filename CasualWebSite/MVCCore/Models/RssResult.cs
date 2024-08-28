using MVCCore.Models.Attributes;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace MVCCore.Models;

[XmlTag(XmlTagAttribute.XmlTagType.CamelCase)]
[XmlRoot("rss")]
public class RssResult
{
    public readonly Channel channel;

    [XmlAttribute]
    public string version = "2.0";

    public RssResult(IEnumerable<RssResultItem> items)
    {
        channel = new Channel
        {
            items = items.ToList()
        };
    }

    public RssResult()
    {

    }
}

[XmlRoot("channel")]
public class Channel
{
    [XmlElement("item")]
    public required List<RssResultItem> items { get; init; }
}

public class RssResultItem
{
    public required string Title { get; set; }

    public required string Link { get; set; }

    [XmlCData]
    public required string Description { get; set; }

    [XmlFormat("R")]
    public required DateTime PubDate { get; set; }
}
