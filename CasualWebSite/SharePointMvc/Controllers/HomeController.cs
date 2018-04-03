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
                description = string.Format("Seed: {0}, Leech: {1}, Size: {2}", x.Seeds, x.Leechers, x.Size),
                link = x.Magnet,
                pubDate = x.UploadDate,
                title = x.Name,
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
        public ActionResult FixNyaaFiltering()
        {
            string url = "https://nyaa.si/?page=rss&q=violet+evergarden+vivid+-superior&c=1_2&f=1";

            string contents = GetUrlTextData(url);

            XmlDocument document = new XmlDocument();

            document.LoadXml(contents);

            var items = document.GetElementsByTagName("item");

            List<XmlNode> invalidNodes = items.AsEnumerable<XmlNode>().Where(x => !x.GetChildNamed("title").InnerText.Contains("v2")).ToList();

            foreach (var tag in invalidNodes)
            {
                tag.ParentNode.RemoveChild(tag);
            }

            contents = XmlToString(document);

            return Content(contents, "application/rss+xml; charset=UTF-8", Encoding.UTF8);
        }

        [HttpGet]
        public ActionResult MangadeepParseXml(string mangaName)
        {
            string url = "http://www.mangadeep.com/"+mangaName;

            string contents = GetUrlTextData(url);
            
            string startTag = "<ul class=\"lst\">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string ulPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(ulPart);

            var liNodes = document.ChildNodes[0].ChildNodes;
            
            RssResult rssObject = new RssResult(liNodes.AsEnumerable<XmlNode>().Select(liNode => new RssResultItem
            {
                description = "This was parsed from MangeDeep.com",
                link = liNode.GetChildNamed("a").Attributes["href"].Value,
                pubDate = DateTime.Parse(liNode.GetChildNamed("a").ChildNodes.AsEnumerable<XmlNode>().FirstOrDefault(x => x.Attributes["class"].Value == "dte").InnerText),
                title = liNode.GetChildNamed("a").Attributes["title"].Value,
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
                description = x.snippet.description,
                link = string.Format(youtubeWatchBaseUrl, x.id.videoId),
                pubDate = x.snippet.publishedAt,
                title = x.snippet.title,
            }));

            return this.Xml(rssObject);
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

            using (MyWebClient client = new MyWebClient())
            {
                client.Encoding = Encoding.UTF8;
                s = client.DownloadString(url);
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

        #endregion
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }
}
