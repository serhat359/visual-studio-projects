﻿using Extensions;
using Model.Web;
using Newtonsoft.Json;
using SharePointMvc.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Xml;
using ThePirateBay;
using WebModelFactory;

namespace SharePointMvc.Controllers
{
    [HandleError]
    public class HomeController : ControllerBase<HomeModelFactory>
    {
        #region Get Methods

        [HttpGet]
        public ActionResult Index()
        {
            return Json(new { message = "this page is empty" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult SNKEvents()
        {
            string q = "SPイベント";

            string channelId = "UCoNJaq5hd0jgAVPFWZ5Ry-A";

            return FilterRssResult(q, channelId);
        }

        [HttpGet]
        public ActionResult YoutubeBeyblade()
        {
            string q = "Season 1";

            string channelId = "UCUTPOb8or0K-VC1Xp3FcDWg";

            return FilterRssResult(q, channelId);
        }

        [HttpGet]
        public ActionResult YoutubeMerryChristmas()
        {
            string q = "オーディオドラマ";

            string channelId = "UCpRh2xmGtaVhFVuyCB271pw";

            return FilterRssResult(q, channelId);
        }

        [HttpGet]
        public ActionResult PirateBayRSS(string query, string containing)
        {
            if (query.IsNullOrEmpty())
            {
                return Json(new { errorMessage = "please specify query and containing parameters" }, JsonRequestBehavior.AllowGet);
            }

            IEnumerable<Torrent> torrents = Tpb.Search(new Query(query, 0, QueryOrder.BySeeds));

            if (containing.IsNullOrEmpty())
                containing = query;

            containing = containing.ToLower();

            string[] containingParts = containing.Split(' ');

            foreach (string part in containingParts)
            {
                torrents = torrents.Where(x => x.Name.ToLower().Contains(part));
            }

            RssResult rssObject = new RssResult(torrents.Select(x => new RssResultItem
            {
                Description = string.Format("Seed: {0}, Leech: {1}, Size: {2}", x.Seeds, x.Leechers, x.Size),
                Link = x.Magnet,
                PubDate = x.UploadDate,
                Title = x.Name,
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public ActionResult ConvertNyaa(string url)
        {
            var contents = GetUrlTextData(url);

            XmlDocument document = new XmlDocument();

            document.LoadXml(contents);

            var items = document.GetElementsByTagName("item");

            var host = ((System.Web.HttpRequestWrapper)Request).Url.Authority;

            RssResult rssObject = new RssResult(items.Cast<XmlNode>().Select(x => new RssResultItem
            {
                Description = x.GetChildNamed("description").InnerText,
                Link = x.GetChildNamed("link").InnerText.Replace("https:", "http:"),
                PubDate = DateTime.Parse(x.GetChildNamed("pubDate").InnerText),
                Title = x.GetChildNamed("title").InnerText,
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public ActionResult Pokemon()
        {
            PokemonModel model = base.ModelFactory.LoadCasual();

            model.Query = "order by greatest(attack,spattack)*speed desc";

            return View(model);
        }

        [HttpGet]
        public ActionResult Download(string path)
        {
            string root = @"C:\Users\Xhertas\";
            string fullPath = root + path;
            string extension = Path.GetExtension(fullPath);
            long fileSize = (new System.IO.FileInfo(fullPath)).Length;
            FileStream fileStream = new FileStream(fullPath, FileMode.Open);

            long contentLength;
            string rangeResult = Request.Params["HTTP_RANGE"];
            if (rangeResult == null)
            {
                contentLength = fileSize;
            }
            else
            {
                long bytesToSkip = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                fileStream.Position = bytesToSkip;

                long startbyte = bytesToSkip;
                long endbyte = fileSize - 1;
                contentLength = endbyte - startbyte + 1;
                Response.StatusCode = 206;
                Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fileSize));
            }

            Response.BufferOutput = false;
            Response.AddHeader("Content-Length", contentLength.ToString());

            return File(fileStream, "application/unknown", "new file" + extension);
        }

        [HttpGet]
        public ActionResult DownloadEncrypted(string path)
        {
            try
            {
                string root = @"C:\Users\Xhertas\";
                string fullPath = root + path;
                string extension = Path.GetExtension(fullPath);
                long fileSize = (new System.IO.FileInfo(fullPath)).Length;
                var fileStream = new EncryptStream(() => new FileStream(fullPath, FileMode.Open));

                long contentLength;
                string rangeResult = Request.Params["HTTP_RANGE"];
                if (rangeResult == null)
                {
                    contentLength = fileSize;
                }
                else
                {
                    long bytesToSkip = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);

                    fileStream.Position = bytesToSkip;

                    long startbyte = bytesToSkip;
                    long endbyte = fileSize - 1;
                    contentLength = endbyte - startbyte + 1;
                    Response.StatusCode = 206;
                    Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fileSize));
                }

                Response.BufferOutput = false;
                Response.AddHeader("Content-Length", contentLength.ToString());

                return File(fileStream, "application/unknown", "new file" + extension);
            }
            catch (Exception e)
            {
                var logPath = @"C:\Users\Xhertas\Desktop\iislogs.txt";

                System.IO.File.AppendAllLines(logPath, new string[] {
                    "Error on DownloadEncrypted",
                    e.Message,
                    e.StackTrace,
                    e.InnerException?.Message,
                    e.InnerException?.StackTrace
                });

                return Content("There was an error, check iislogs.txt on desktop");
            }
        }

        [HttpGet]
        public ActionResult FixAnimeNews()
        {
            string url = "http://www.animenewsnetwork.com/news/rss.xml";

            string contents = GetUrlTextData(url)
                .Replace("animenewsnetwork.cc", "animenewsnetwork.com")
                .Replace("http://", "https://");

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
        }

        [HttpGet]
        public ActionResult FixTomsNews()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/news.xml";

            return FixToms(url);
        }

        [HttpGet]
        public ActionResult FixTomsNewsAlternative()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/all.xml";

            return FixToms(url, "/news/");
        }

        [HttpGet]
        public ActionResult FixTomsArticles()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/articles.xml";

            return FixToms(url);
        }

        [HttpGet]
        public ActionResult FixTomsArticlesAlternative()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/all.xml";

            return FixToms(url, "/reviews/");
        }

        [HttpGet]
        public ActionResult FixTomsArticlesManual()
        {
            string[] keywords = {
                "review/",
                "reference/",
                "feature/",
                "how-to/",
                "opinion/",
                "round-up/",
                "best-picks/",
                "buying-guide/"
            };

            keywords = keywords.Select(x => "https://www.tomshardware.com/articles/" + x).ToArray();

            var ss = keywords.Select(url => GetRssObjectFromTomsUrl(url));

            var rssObject = new RssResult(ss.SelectMany(x => x.channel.items).Where(x => !x.Link.Contains("/news/")).OrderByDescending(x => x.PubDate));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public ActionResult FixTomsAll()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/all.xml";

            return FixToms(url);
        }

        [HttpGet]
        public ActionResult FixTomsNewsManualParse()
        {
            string url = "https://www.tomshardware.com/articles/news/";

            var rssObject = GetRssObjectFromTomsUrl(url);

            return this.Xml(rssObject);
        }

        private static RssResult GetRssObjectFromTomsUrl(string url)
        {
            string baseUrl = "https://www.tomshardware.com";

            string contents = GetUrlTextData(url);

            string startTag = "<ul class=\"listing-items\">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string ulPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);
            ulPart = ulPart.Replace(" itemscope ", "  ");

            while (true)
            {
                var metaIndex = ulPart.IndexOf("<meta");

                if (metaIndex < 0)
                    break;

                var metaEndIndex = ulPart.IndexOf(">", metaIndex) + 1;

                ulPart = ulPart.Replace(ulPart.Substring(metaIndex, metaEndIndex - metaIndex), "");
            }

            XmlDocument document = new XmlDocument();
            document.LoadXml(ulPart);

            var liNodes = document.ChildNodes[0].ChildNodes;

            RssResult rssObject = new RssResult(liNodes.Cast<XmlNode>().Select(liNode =>
            {
                var firstDegree = liNode.ChildNodes.Cast<XmlNode>();
                var secondDegree = firstDegree.SelectMany(x => x.ChildNodes.Cast<XmlNode>());
                var thirdDegree = secondDegree.SelectMany(x => x.ChildNodes.Cast<XmlNode>());
                var fourthtDegree = thirdDegree.SelectMany(x => x.ChildNodes.Cast<XmlNode>());

                var aNode = thirdDegree.First(x => x.Name == "a");
                var link = baseUrl + aNode.Attributes["href"].Value;

                var imgNode = aNode.ChildNodes.Cast<XmlNode>().First(x => x.Name == "img");
                var imgSrc = imgNode.Attributes["data-src"].InnerText;
                
                var img = $"<a href=\"{link}\"><img src=\"{imgSrc}\" /></a>";

                return new RssResultItem
                {
                    Description = $"<![CDATA[{img}]]>",
                    Link = link,
                    PubDate = DateTime.Parse(fourthtDegree.First(x => x.Name == "div").GetChildNamed("time").InnerText),
                    Title = aNode.Attributes["title"].Value,
                };
            }));
            return rssObject;
        }

        [HttpGet]
        public ActionResult FixNyaaFullMetal(int filter)
        {
            string url = "https://nyaa.si/?page=rss&q=full+metal+panic+horrible+720&c=1_2&f=0";

            string contents = GetUrlTextData(url);

            XmlDocument document = new XmlDocument();

            document.LoadXml(contents);

            var items = document.GetElementsByTagName("item");

            List<XmlNode> invalidNodes = items.Cast<XmlNode>().Where(x =>
            {
                string text = x.GetChildNamed("title").InnerText;
                int number = int.Parse(text.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries)[1].Split(' ', '.')[0]);
                return number < filter;
            }).ToList();

            foreach (var node in invalidNodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            contents = XmlToString(document);

            contents = XmlEncodeForHtml(contents);

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
        }

        [HttpGet]
        public ActionResult FixNyaaFiltering()
        {
            string url = "https://nyaa.si/?page=rss&q=violet+evergarden+vivid+-superior&c=1_2&f=1";

            string contents = GetUrlTextData(url);

            XmlDocument document = new XmlDocument();

            document.LoadXml(contents);

            var items = document.GetElementsByTagName("item");

            List<XmlNode> invalidNodes = items.Cast<XmlNode>().Where(x => !x.GetChildNamed("title").InnerText.Contains("v2")).ToList();

            foreach (var node in invalidNodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            contents = XmlToString(document);

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
        }

        [HttpGet]
        public ActionResult MangadeepParseXml(string id)
        {
            string mangaName = id;

            if (string.IsNullOrWhiteSpace(mangaName))
                throw new Exception("manganame can't be empty");

            string url = "http://www.mangadeep.com/" + mangaName;

            string contents = GetUrlTextData(url);

            string startTag = "<ul class=\"lst\">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string ulPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(ulPart);

            var liNodes = document.ChildNodes[0].ChildNodes;

            RssResult rssObject = new RssResult(liNodes.Cast<XmlNode>().Select(liNode => new RssResultItem
            {
                Description = "This was parsed from MangeDeep.com",
                Link = liNode.GetChildNamed("a").Attributes["href"].Value,
                PubDate = DateTime.Parse(liNode.GetChildNamed("a").ChildNodes.Cast<XmlNode>().FirstOrDefault(x => x.Attributes["class"].Value == "dte").Attributes["title"].Value.Replace("Published on ", "")),
                Title = liNode.GetChildNamed("a").Attributes["title"].Value,
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public ActionResult MangagoParseXml(string id)
        {
            string mangaName = id;

            if (string.IsNullOrWhiteSpace(mangaName))
                throw new Exception("manganame can't be empty");

            string url = "http://www.mangago.me/read-manga/" + mangaName;

            string contents = GetUrlTextData(url, client =>
                {
                    client.Headers.Add(HttpRequestHeader.Cookie, "__utma=5576751.1839360451.1493553979.1493715144.1549998116.8; __atuvc=2%7C7; __unam=57d317c-168e3164884-26803dd2-4; __cfduid=d31bdce544f944651c31af6da4d152d511569740230; _mtma=_uc15704533962020.18596782751504148; cf_clearance=ede1830af8b2df75f54b3b7cbaa43a438561c991-1570641185-0-150; PHPSESSID=q61v27jf87jt3mcektbs7ls4v4; __utmd=a4762f3196be64c16ed72b4aad6b34d3");
                    client.Headers.Add(HttpRequestHeader.Host, "www.mangago.me");
                    client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:60.9) Gecko/20100101 Goanna/4.4 Firefox/60.9 PaleMoon/28.7.1");
                    client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                    client.Headers.Add(HttpRequestHeader.AcceptLanguage, "en,en-US;q=0.8,tr-TR;q=0.5,tr;q=0.3");
                    client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");
                    client.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
                }
            );

            string startTag = "<table class=\"listing\" id=\"chapter_table\">";
            string endTag = "</table>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string tablePart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(tablePart);

            var trNodes = document.ChildNodes[0].ChildNodes[0].ChildNodes;

            RssResult rssObject = new RssResult(trNodes.Cast<XmlNode>().Select(trNode =>
            {
                var aNode = trNode.ChildNodes[0].ChildNodes[0].ChildNodes[0];
                var secondTdNode = trNode.ChildNodes[1];
                return new RssResultItem
                {
                    Description = "This was parsed from Mangago.me",
                    Link = aNode.Attributes["href"].Value,
                    PubDate = DateTime.Parse(secondTdNode.InnerText),
                    Title = aNode.InnerText.Replace("<b>", "").Replace("</b>", "").Replace("\t", "").Replace("\n", ""),
                };
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public ActionResult Parse1337FailedAttempt(string id, string contains)
        {
            var today = DateTime.Today;

            id = id.Replace(' ', '+');
            var url = $"https://1337x.to/search/{id}/1/";

            string contents = GetUrlTextData(url);

            string startTag = "<table class=\"table-list table table-responsive table-striped\">";
            string endTag = "</table>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string tablePart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(tablePart);

            var rows = document.ChildNodes[0].ChildNodes[1].ChildNodes;

            var list = new List<RssResultItem>();
            foreach (XmlNode row in rows)
            {
                var name = row.ChildNodes[0].ChildNodes[1].InnerText;
                var seed = row.ChildNodes[1].InnerText;
                var leech = row.ChildNodes[2].InnerText;
                var size = row.ChildNodes[4].InnerXml;
                size = size.Substring(0, size.IndexOf('<'));

                if (contains != null && !name.Contains(contains))
                {
                    continue;
                }

                var description = $"{name} {size} S:{seed} L:{leech}";

                list.Add(new RssResultItem
                {
                    Description = description,
                    PubDate = today,
                    Title = description,
                    Link = ""
                });
            }

            RssResult rssResult = new RssResult(list);

            return this.Xml(rssResult);
        }

        [HttpGet]
        public ActionResult DownloadNyaa(string link)
        {
            var data = GetUrlTextDataArray(link);

            return File(data, "application/x-bittorrent");
        }

        #endregion

        #region Post Methods
        [HttpPost]
        public ActionResult Pokemon(PokemonModel request)
        {
            PokemonModel model = ModelState.IsValid ? base.ModelFactory.LoadCasual(request) : base.ModelFactory.LoadCasual();

            return View(model);
        }
        #endregion

        #region Private Methods

        private ActionResult FilterRssResult(string q, string channelId)
        {
            string jsonResult = GetJsonSearchResult(q, channelId);

            YoutubeSearchResult youtubeSearchResult = JsonConvert.DeserializeObject<YoutubeSearchResult>(jsonResult);

            string youtubeWatchBaseUrl = "https://www.youtube.com/watch?v={0}";

            RssResult rssObject = new RssResult(youtubeSearchResult.items.Select(x => new RssResultItem
            {
                Description = x.snippet.description,
                Link = string.Format(youtubeWatchBaseUrl, x.id.videoId),
                PubDate = x.snippet.publishedAt,
                Title = x.snippet.title,
            }));

            return this.Xml(rssObject);
        }

        private ActionResult FixToms(string url, string linkContains = null)
        {
            string contents = GetUrlTextData(url);

            contents = contents.Replace(".co.uk", ".com");

            //contents = IndentXml(contents);

            XmlDocument document = new XmlDocument();

            document.LoadXml(contents);

            var items = document.GetElementsByTagName("item").Cast<XmlNode>();

            items.Where(x => x.GetChildNamed("link").InnerXml.Contains("tomshardware.co.uk")).ToList().ForEach(node =>
            {
                node.ParentNode.RemoveChild(node);
            });

            if (linkContains != null)
            {
                items.Where(x => !x.GetChildNamed("link").InnerXml.Contains(linkContains)).ToList().ForEach(node =>
                {
                    node.ParentNode.RemoveChild(node);
                });
            }

            items = document.GetElementsByTagName("item").Cast<XmlNode>();

            var iterator = items.GetEnumerator();

            List<XmlNode> invalidNodes = new List<XmlNode>();

            XmlNode oldOne = null;
            while (iterator.MoveNext())
            {
                var curr = iterator.Current;

                if (oldOne != null)
                {
                    var oldTitle = oldOne.GetChildNamed("title").InnerText;
                    var currTitle = curr.GetChildNamed("title").InnerText;

                    if (oldTitle.Equals(currTitle))
                    {
                        invalidNodes.Add(oldOne);
                    }
                }

                var link = curr.GetChildNamed("link");
                link.InnerText = link.InnerText.Replace("#xtor=RSS-5", "");

                oldOne = curr;
            }

            foreach (var node in invalidNodes)
            {
                node.ParentNode.RemoveChild(node);
            }

            contents = XmlToString(document);

            contents = contents.Replace("&#039;", "'");

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
        }

        private static string GetJsonSearchResult(string q, string channelId)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("order", "date");
            parameters.Add("part", "snippet");
            parameters.Add("channelId", channelId);
            parameters.Add("key", "AIzaSyBrbMk0ZR5638IrTlVBTra7NLYdoBjUt2o");
            parameters.Add("maxResults", "10");
            parameters.Add("q", Uri.EscapeUriString(q));

            string baseurl = "https://www.googleapis.com/youtube/v3/search";

            string wholeUrl = string.Format("{0}?{1}", baseurl, string.Join("&", parameters.Select(pair => pair.Key + "=" + pair.Value)));

            string jsonResult = GetUrlTextData(wholeUrl);
            return jsonResult;
        }

        private static byte[] GetUrlTextDataArray(string url)
        {
            byte[] s;

            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    s = client.DownloadData(url);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return s;
        }

        private static string GetUrlTextData(string url, Action<WebClient> extraAction = null)
        {
            string s;

            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    client.Encoding = Encoding.UTF8;

                    extraAction?.Invoke(client);

                    s = client.DownloadString(url);
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return s;
        }

        private static string XmlEncodeForHtml(string str)
        {
            StringBuilder stringBuilder = new StringBuilder();

            int i = 0;

            while (i < str.Length)
            {
                var c = str[i];

                if (c == '<')
                {
                    //var finishingIndex = str.IndexOf('>', i + 1);

                    var finishingIndex = FindClosingIndex(str, i);
                    if (finishingIndex == -1)
                        throw new Exception();

                    stringBuilder.Append(str.Substring(i, finishingIndex + 1 - i));

                    i = finishingIndex + 1;
                    continue;
                }

                int startingIndex = i;
                int endingIndex = i;

                while (str[endingIndex] != '<')
                {
                    endingIndex++;
                }

                string sub = str.Substring(i, endingIndex - i);
                stringBuilder.Append(MyXmlSerializer.EscapeXMLValue(sub, true));
                i = endingIndex;
            }

            return stringBuilder.ToString();
        }

        private static string XmlToString(XmlDocument document)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);

            try
            {
                writer.Formatting = System.Xml.Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXML = sReader.ReadToEnd();

                result = formattedXML.Replace("  ", "\t");

                mStream.Close();
                writer.Close();
            }
            catch (XmlException)
            {
            }

            return result;
        }

        private static string IndentXml(string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                // Load the XmlDocument with the XML.
                document.LoadXml(xml);

                writer.Formatting = System.Xml.Formatting.Indented;

                // Write the XML into a formatting XmlTextWriter
                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                // Have to rewind the MemoryStream in order to read
                // its contents.
                mStream.Position = 0;

                // Read MemoryStream contents into a StreamReader.
                StreamReader sReader = new StreamReader(mStream);

                // Extract the text from the StreamReader.
                string formattedXML = sReader.ReadToEnd();

                result = formattedXML.Replace("  ", "\t");

                mStream.Close();
                writer.Close();
            }
            catch (XmlException)
            {
            }

            return result;
        }

        private static int FindClosingIndex(string str, int startingIndex)
        {
            int i = startingIndex;
            int count = 0;
            while (true)
            {
                var c = str[i];
                if (c == '<')
                {
                    count++;
                }
                else if (c == '>')
                {
                    count--;
                    if (count == 0)
                    {
                        return i;
                    }
                }
                i++;
            }
        }

        #endregion
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            return request;
        }
    }
}
