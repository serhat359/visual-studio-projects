namespace MVCCore
{
    public class Resource1
    {
        public const string RssTemplate = @"
<rss xmlns:atom=""http://www.w3.org/2005/Atom"" xmlns:nyaa=""https://nyaa.si/xmlns/nyaa"" version=""2.0"">
	<channel>
		<title>Nyaa - All Rss Links - Torrent File RSS</title>
		<description>RSS Feed for Nyaa</description>
		<link>https://nyaa.si/</link>
		<atom:link href=""https://nyaa.si/?page=rss"" rel=""self"" type=""application/rss+xml"" />
		
	</channel>
</rss>
";
    }
}
