using Extensions;
using Model.Web;
using Newtonsoft.Json;
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
        public ActionResult FixAnimeNews()
        {
            string url = "http://www.animenewsnetwork.com/news/rss.xml";

            string contents = GetUrlTextData(url)
                .Replace("animenewsnetwork.cc", "animenewsnetwork.com")
                .Replace("http://", "https://");

            return Content(contents, "application/rss+xml; charset=UTF-8", Encoding.UTF8);
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
        public ActionResult FixTomsAll()
        {
            string url = "https://www.tomshardware.com/feeds/rss2/all.xml";

            return FixToms(url);
        }

        [HttpGet]
        public ActionResult FixTomsNewsManualParse()
        {
            string url = "https://www.tomshardware.com/articles/news/";
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

                return new RssResultItem
                {
                    Description = "",
                    Link = baseUrl + aNode.Attributes["href"].Value,
                    PubDate = DateTime.Parse(fourthtDegree.First(x => x.Name == "div").GetChildNamed("time").InnerText),
                    Title = aNode.Attributes["title"].Value,
                };
            }));

            return this.Xml(rssObject);
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

            return Content(contents, "application/rss+xml; charset=UTF-8", Encoding.UTF8);
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

            return Content(contents, "application/rss+xml; charset=UTF-8", Encoding.UTF8);
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

        private static string GetUrlTextData(string url)
        {
            string s;

            try
            {
                using (MyWebClient client = new MyWebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    s = client.DownloadString(url);
                }
            }
            catch (Exception e)
            {


                throw;
            }

            return s;
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

        #endregion
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            return request;
        }
    }
}
