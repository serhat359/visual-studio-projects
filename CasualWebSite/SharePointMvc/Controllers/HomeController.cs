using Business;
using Extensions;
using Model.Web;
using Newtonsoft.Json;
using SharePointMvc.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using WebModelFactory;

namespace SharePointMvc.Controllers
{
    [HandleError]
    [AllowCORS]
    public class HomeController : ControllerBase<HomeModelFactory>
    {
        readonly (string monthName, int monthNumber)[] months = { ("January", 1), ("February", 2), ("March", 3), ("April", 4), ("May", 5), ("June", 6),
            ("July", 7), ("August", 8), ("September", 9), ("October", 10), ("November", 11), ("December", 12) };

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
        public ActionResult GetAnimeNewsCookie()
        {
            object cookieValue = CacheHelper.Get<string>(CacheHelper.MALCookie, () => "", CacheHelper.MALCookieTimeSpan);

            return View(cookieValue);
        }

        [HttpPost]
        public ActionResult GetAnimeNewsCookie(string value)
        {
            CacheHelper.Set(CacheHelper.MALCookie, value, CacheHelper.MALCookieTimeSpan);
            object cookieValue = value;
            return View(cookieValue);
        }

        [HttpGet]
        public ActionResult FixAnimeNews()
        {
            string url = "https://www.animenewsnetwork.com/news/rss.xml?ann-edition=us";

            string contents = GetUrlTextData(url, x =>
            {
                x.Headers.Add(HttpRequestHeader.Host, "www.animenewsnetwork.com");
                x.Headers.Add(HttpRequestHeader.Cookie, CacheHelper.Get(CacheHelper.MALCookie, () => "", CacheHelper.MALCookieTimeSpan));
                x.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.1; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0");
            })
                .Replace("animenewsnetwork.cc", "animenewsnetwork.com")
                .Replace("http://", "https://");

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
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
            Func<ActionResult> initializer = () =>
            {
                string[] urls = { "https://www.tomshardware.com/reviews",
                              "https://www.tomshardware.com/reviews/page/2",
                              "https://www.tomshardware.com/reference",
                              "https://www.tomshardware.com/features",
                              "https://www.tomshardware.com/how-to",
                              "https://www.tomshardware.com/round-up",
                              "https://www.tomshardware.com/best-picks",
                             };

                var threads = urls.Select(url => Task.Run(() => GetRssObjectFromTomsUrlNew(url))).ToList();

                var ss = threads.Select(x => x.Result).SelectMany(x => x);

                var rssObject = new RssResult(ss.Where(x => !x.Link.Contains("/news/")).OrderByDescending(x => x.PubDate));

                var xmlResult = this.Xml(rssObject);

                return xmlResult;
            };

            var xmlResultResult = CacheHelper.Get<ActionResult>(CacheHelper.TomsArticlesKey, initializer, TimeSpan.FromHours(2));

            return xmlResultResult;
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
            string[] urls = { "https://www.tomshardware.com/news", "https://www.tomshardware.com/news/page/2" };

            var elements = urls.SelectMany(url => GetRssObjectFromTomsUrlNew(url));

            var rssObject = new RssResult(elements.OrderByDescending(c => c.PubDate));

            return this.Xml(rssObject);
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
        public ActionResult Parse1337FailedAttempt(string id, string[] contains)
        {
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

            var baseDomain = Request.Url.Scheme + "://" + Request.Url.Authority;
            var list = new List<UtorrentRssResultItem>();
            foreach (XmlNode row in rows)
            {
                var name = row.ChildNodes[0].ChildNodes[1].InnerText;
                var link = row.ChildNodes[0].ChildNodes[1].Attributes["href"].Value;
                var seed = row.ChildNodes[1].InnerText;
                var leech = row.ChildNodes[2].InnerText;
                var time = row.ChildNodes[3].InnerText;
                var size = row.ChildNodes[4].InnerXml;
                size = size.Substring(0, size.IndexOf('<'));

                if (contains?.Any(x => name.ContainsCaseInsensitive(x)) == true)
                {
                    continue;
                }

                var description = $"{name} {size} Seed:{seed} Leech:{leech}";

                Func<string, DateTime> todayDateParser = timeParam =>
                {
                    timeParam = timeParam.ToLowerInvariant();
                    var isAm = timeParam.Contains("am");
                    timeParam = timeParam.Substring(0, timeParam.IndexOfFirst(x => x >= 'a' && x <= 'z'));
                    var timeparts = timeParam.Split(':');
                    var hour = int.Parse(timeparts[0]) + (isAm ? 0 : 12);
                    var minute = int.Parse(timeparts[1]);
                    var newDate = DateTime.Today + new TimeSpan(hour, minute, seconds: 0);
                    return newDate;
                };
                Func<string, DateTime> yesterdayDateParser = timeParam =>
                {
                    var timeparts = timeParam.Split(new[] { ' ', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    int month = months.First(x => x.monthName.StartsWith(timeparts[1])).monthNumber;
                    int year = DateTime.Today.Year;
                    var dayPart = timeparts[2];
                    int indexOfFirst = dayPart.IndexOfFirst(x => x < '0' || x > '9');
                    int day = int.Parse(dayPart.Substring(0, indexOfFirst));
                    var newDate = new DateTime(year: year, month: month, day: day);
                    return newDate;
                };
                Func<string, DateTime> regularDateParser = timeParam =>
                {
                    var timeparts = timeParam.Split(new[] { ' ', '.', '\'' }, StringSplitOptions.RemoveEmptyEntries);

                    int month = months.First(x => x.monthName.StartsWith(timeparts[0])).monthNumber;
                    int year = 2000 + int.Parse(timeparts[2]);
                    var dayPart = timeparts[1];
                    int indexOfFirst = dayPart.IndexOfFirst(x => x < '0' || x > '9');
                    int day = int.Parse(dayPart.Substring(0, indexOfFirst));
                    var newDate = new DateTime(year: year, month: month, day: day);
                    return newDate;
                };

                DateTime date = time.Contains("'") ? regularDateParser(time) : time.Contains(":") ? todayDateParser(time) : yesterdayDateParser(time);

                var newLink = baseDomain + $"/Home/{nameof(Get1337Torrent)}?link={Uri.EscapeDataString(link)}";

                list.Add(new UtorrentRssResultItem
                {
                    Description = description,
                    PubDate = date,
                    Title = description,
                    Link = newLink,
                    Guid = newLink
                });
            }

            list = list.OrderByDescending(c => c.PubDate).ToList();

            var rssResult = new UtorrentRssResult(list);

            return this.Xml(rssResult, igroneXmlVersion: true);
        }

        [HttpGet]
        public ActionResult Get1337Torrent(string link)
        {
            var linkBase = "https://1337x.to";
            link = linkBase + link;

            string contents = GetUrlTextData(link);

            var itorrentsIndex = contents.IndexOf("ITORRENTS MIRROR");
            var indexOfStart = contents.LastIndexOf("<a", itorrentsIndex);
            string endTag = "</a>";
            var indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string linkPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(linkPart);
            var torrentLink = document.ChildNodes[0].Attributes["href"].Value;
            torrentLink = torrentLink.Replace("http://", "https://");

            var torrentContent = GetUrlTextDataArray(torrentLink);
            var torrentFileName = torrentLink.Substring(torrentLink.LastIndexOf("/"));

            return TorrentFile(torrentContent, torrentFileName);
        }

        [HttpGet]
        public ActionResult DownloadNyaa(string link)
        {
            var data = GetUrlTextDataArray(link);

            return File(data, "application/x-bittorrent");
        }

        [HttpGet]
        public ActionResult LHScanParse(string id)
        {
            string mangaName = id;

            if (string.IsNullOrWhiteSpace(mangaName))
                throw new Exception("manganame can't be empty");

            string url = $"https://lhscan.net/{mangaName}.html";
            string baseLink = "https://lhscan.net/";

            string contents = GetUrlTextData(url);

            string startTag = "<table class=\"table table-hover\">";
            string endTag = "</table>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string tablePart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(tablePart);

            (string timeName, double timedays)[] dates = { ("day", 1.0), ("week", 7.0), ("month", 30.0), ("year", 365.0), ("hour", 1.0 / 24.0) };
            DateTime today = DateTime.Today;

            var result = new RssResult(document.SearchByTag("tbody").ChildNodes.Cast<XmlNode>().Select(x =>
            {
                var dateString = x.SearchByTag("time").InnerText.ToLowerInvariant();
                var days = dates.First(c => dateString.Contains(c.timeName)).timedays;

                var realDays = int.Parse(dateString.Split(' ')[0]) * days;

                return new RssResultItem
                {
                    Description = "This was parsed from LHscan.net",
                    Link = baseLink + x.SearchByTag("a").Attributes["href"].Value,
                    PubDate = today.AddDays(-realDays),
                    Title = x.SearchByTag("b").InnerText
                };
            }));

            return Xml(result);
        }

        [HttpGet]
        public ActionResult MangaTownParse(string id)
        {
            string mangaName = id;

            if (string.IsNullOrWhiteSpace(mangaName))
                throw new Exception("manganame can't be empty");

            string url = $"https://m.mangatown.com/manga/{mangaName}/";
            string baseLink = "https://m.mangatown.com";

            string contents = GetUrlTextData(url);

            string startTag = "<ul class=\"detail-ch-list\">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string tablePart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            XmlDocument document = new XmlDocument();
            document.LoadXml(tablePart);

            var today = DateTime.Today;

            var result = new RssResult(document.ChildNodes[0].ChildNodes.Cast<XmlNode>().Select(x =>
            {
                var dateString = x.SearchByTag("span", @class: "time").InnerText;

                if (!DateTime.TryParse(dateString, out var date))
                {
                    if (dateString == "Today")
                        date = today;
                    else if (dateString == "Yesterday")
                        date = today.AddDays(-1);
                    else
                        throw new Exception($"could not parse string to date: {dateString}");
                }

                var link = x.SearchByTag("a");

                return new RssResultItem
                {
                    Description = "This was parsed from mangatown.com",
                    Link = link.Attributes["href"].Value,
                    PubDate = date,
                    Title = mangaName + " " + link.FirstChild.InnerText
                };
            }).Take(10));

            return Xml(result);
        }

        [HttpGet]
        public ActionResult MankinTrad()
        {
            string url = $"http://mankin-trad.net/feed/";

            string contents = GetUrlTextData(url);
            contents = contents.Replace("&-", "&amp;-");

            contents = FixMankinTradImgs(contents);

            new XmlDocument().LoadXml(contents);

            return Content(contents, "application/xml");
        }

        [HttpGet]
        public ActionResult GetOmoriResults()
        {
            var url = "https://www.borderless-house.com/jp/sharehouse/omori/";

            string baseLink = "https://www.borderless-house.com";

            string contents = GetUrlTextData(url);

            string divStart = "<div id=\"panel-4\"";

            int indexOfDivStart = contents.IndexOf(divStart);

            string startTag = "<tbody>";
            string endTag = "</tbody>";

            int indexOfStart = contents.IndexOf(startTag, indexOfDivStart);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string tablePart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            tablePart = FixUseLinks(tablePart);

            XmlDocument document = new XmlDocument();
            document.LoadXml(tablePart);

            var elements = document.ChildNodes[0].ChildNodes.Cast<XmlNode>()
                .Where(x => !string.IsNullOrEmpty(x.InnerText))
                .Where(x => !x.InnerText.Contains("Female"))
                .Where(x => !x.InnerText.Contains("Japanese nationality"))
                .Where(x => !x.InnerText.Contains("Room for 2"))
                .Where(x => !x.InnerText.Contains("Room for 3"))
                .Where(x => !x.InnerText.Contains("Room for 4"))
                .Where(x =>
                {
                    var areaText = x.ChildNodes.Cast<XmlNode>().Where(y => y.InnerText.Contains("㎡")).FirstOrDefault()?.InnerText;

                    if (areaText == null)
                        return false;

                    var area = double.Parse(areaText.Replace("㎡", ""));

                    return area > 8;
                });

            var date = DateTime.Today;

            return Xml(new RssResult(elements.Select(x => new RssResultItem
            {
                Description = x.InnerText,
                PubDate = date,
                Title = "Omori",
                Link = url
            })));
        }

        [HttpGet]
        public ActionResult GenerateRssResult()
        {
            var cacheTimespan = TimeSpan.FromMinutes(15);

            Func<ContentResult> initializerFunction = () =>
            {
                var links = MyTorrentRssHelper.Instance(Request.PhysicalApplicationPath).GetLinks().Keys;

                var allLinks = new List<(DateTime date, XmlNode node)>();

                var tasks = links.Select(url => Task.Run(() =>
                {
                    var key = CacheHelper.MyRssKey + ":" + url;
                    var val = CacheHelper.Get(key, () => GetUrlTextData(url), cacheTimespan);
                    return val;
                })).ToArray();

                foreach (var task in tasks)
                {
                    try
                    {
                        var result = task.Result;
                        var xml = new XmlDocument();
                        xml.LoadXml(result);

                        var itemNodes = xml.ChildNodes[0].ChildNodes[0].ChildNodes.Cast<XmlNode>().Where(x => x.Name == "item").ToList();
                        foreach (var itemNode in itemNodes)
                        {
                            var date = DateTime.Parse(itemNode.SearchByTag("pubDate").InnerText);

                            // This line was added for browser compatibility, should not be used with bittorrent clients
                            itemNode.SearchByTag("link").InnerText = itemNode.SearchByTag("guid").InnerText;

                            allLinks.Add((date, itemNode));
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }

                var allNodes = allLinks.OrderByDescending(x => x.date).Select(x => x.node).ToList();

                var newXml = new XmlDocument();
                newXml.LoadXml(Resource1.RssTemplate);

                var itemNodesParent = newXml.ChildNodes[0].ChildNodes[0];
                foreach (var node in allNodes)
                {
                    itemNodesParent.AppendChild(newXml.ImportNode(node, deep: true));
                }

                var contentResult = Content(newXml.Beautify(), "application/xml");

                return contentResult;
            };

            return CacheHelper.Get<ContentResult>(CacheHelper.MyRssKey, initializerFunction, cacheTimespan);
        }

        [HttpGet]
        public ActionResult GetRssLinks()
        {
            var links = MyTorrentRssHelper.Instance(Request.PhysicalApplicationPath).GetLinks();

            return View(links);
        }

        [HttpPost]
        public ActionResult AddRssLink(string link, string name)
        {
            MyTorrentRssHelper.Instance(Request.PhysicalApplicationPath).AddLink(link: link, name: name);
            CacheHelper.Delete(CacheHelper.MyRssKey);

            return RedirectToAction(nameof(GetRssLinks));
        }

        [HttpGet]
        public ActionResult DeleteRssLink(string link)
        {
            MyTorrentRssHelper.Instance(Request.PhysicalApplicationPath).RemoveLink(link);
            CacheHelper.Delete(CacheHelper.MyRssKey);

            return RedirectToAction(nameof(GetRssLinks));
        }

        [HttpGet]
        public ActionResult BypassCors(string q)
        {
            if (string.IsNullOrEmpty(q)) throw new ArgumentNullException(nameof(q));

            using (MyWebClient client = new MyWebClient())
            {
                var res = client.GetResponse(q);
                return File(res.GetResponseStream(), res.ContentType);
            }
        }

        [HttpGet]
        public ActionResult DownloadMankinTrad(string link, string downloadName)
        {
            var contents = GetUrlTextData(link);

            int indexOfDivStart = 0;

            string startTag = "<div class=\"reading-content\">";
            string endTag = "                                            </div>";

            int indexOfStart = contents.IndexOf(startTag, indexOfDivStart);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string body = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            body = FixIncompleteImgs(body);

            XmlDocument document = new XmlDocument();
            document.LoadXml(body);

            var threads = new List<(string fileName, MyThread<byte[]> thread)>();

            foreach (XmlNode item in document.GetElementsByTagName("img"))
            {
                var dataSrc = item.Attributes["data-src"];
                if (dataSrc != null)
                {
                    var imgLink = dataSrc.Value.Trim();
                    var fileName = imgLink.Substring(imgLink.LastIndexOf('/') + 1);
                    fileName = Uri.UnescapeDataString(fileName);

                    threads.Add((fileName, MyThread.DoInThread(true, () =>
                    {
                        var data = GetUrlTextDataArray(imgLink);
                        return data;
                    })));
                }
            }

            var archiveStream = new MemoryStream();

            using (var zip = new ZipArchive(archiveStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var thread in threads)
                {
                    var data = thread.thread.Await();

                    var entry = zip.CreateEntry(thread.fileName);
                    using (var stream = entry.Open())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
            }

            archiveStream.Seek(0, SeekOrigin.Begin);

            HttpContext.Response.AddHeader("Content-Length", archiveStream.Length.ToString());
            Response.AddHeader("Content-Length", archiveStream.Length.ToString());
            Response.Headers.Add("Content-Length", archiveStream.Length.ToString());

            return File(archiveStream, contentType: "application/zip", fileDownloadName: downloadName);
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

            while (true)
            {
                try
                {
                    using (MyWebClient client = new MyWebClient())
                    {
                        client.Encoding = Encoding.UTF8;

                        extraAction?.Invoke(client);

                        s = client.DownloadString(url);
                        break;
                    }
                }
                catch (WebException e)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw;
                }
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

        private static IEnumerable<RssResultItem> GetRssObjectFromTomsUrlNew(string url)
        {
            string contents = GetUrlTextData(url);

            string startTag = "<section data-next=\"latest\"";
            string endTag = "</section>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string sectionPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);
            sectionPart = sectionPart.Replace(" itemscope ", "  ");
            sectionPart = sectionPart.Replace("&rsquo;", "&apos;");
            sectionPart = sectionPart.Replace("&lsquo;", "&apos;");
            sectionPart = sectionPart.Replace("&rdquo;", "\"");
            sectionPart = sectionPart.Replace("&ldquo;", "\"");
            sectionPart = sectionPart.Replace("&pound;", "£");
            sectionPart = sectionPart.Replace("&sup2;", "²");
            sectionPart = sectionPart.Replace("&mdash;", "—");
            sectionPart = sectionPart.Replace("&plusmn;", "±");
            sectionPart = sectionPart.Replace("&trade;", "™");

            sectionPart = FixIncompleteImgs(sectionPart);

            XmlDocument document = new XmlDocument();
            sectionPart = FixTomsHardwareBrokenQuoteXml(sectionPart);
            sectionPart = FixTomsHardwareBrokenXml(sectionPart);
            document.LoadXml(sectionPart);

            var divs = document.GetAllNodes().Where(c =>
            {
                var classValue = c.Attributes?["class"]?.Value;
                return classValue?.Contains("listingResult small") == true
                    && classValue?.Contains("sponsored") != true;
            }).ToList();

            var elements = divs.Cast<XmlNode>().Select(liNode =>
            {
                var aNode = liNode.SearchByTag("a");
                var link = aNode.Attributes["href"].Value;

                var imgNode = liNode.SearchByTag("img");
                var imgSrc = imgNode.Attributes["data-srcset"]?.InnerText?.BeforeFirst(" ") ?? imgNode.Attributes["data-src"]?.InnerText ?? imgNode.Attributes["src"].InnerText;

                var img = $"<a href=\"{link}\"><img src=\"{imgSrc}\" /></a>";

                return new RssResultItem
                {
                    Description = $"<![CDATA[{img}]]>",
                    Link = link,
                    PubDate = DateTime.Parse(liNode.SearchByTag("time").Attributes["datetime"].Value),
                    Title = liNode.SearchByTag("h3").InnerText,
                };
            });

            return elements;
        }

        private static string FixTomsHardwareBrokenXml(string sectionPart)
        {
            var ariaText = "aria-label";
            var i = 0;
            var ss = new StringBuilder();

            while (true)
            {
                var i1 = sectionPart.IndexOf(ariaText, i);
                if (i1 < 0)
                    break;

                var i2 = sectionPart.IndexOf('"', i1);
                var i3 = sectionPart.IndexOf('"', i2 + 1);
                if (sectionPart[i3 + 1] == '>')
                {
                    i += ariaText.Length;
                    continue;
                }
                else
                {
                    var labelPart = sectionPart.Substring(i2, i3 - i2 + 1);
                    var restPart = sectionPart.Substring(sectionPart.IndexOf('>', i2));
                    ss.Append(sectionPart.Substring(0, i2));
                    ss.Append(labelPart);
                    sectionPart = restPart;
                    i = 0;
                }
            }

            sectionPart = sectionPart.Replace("&alpha;", "");

            ss.Append(sectionPart);

            return ss.ToString();
        }

        private static string FixTomsHardwareBrokenQuoteXml(string sectionPart)
        {
            var ariaText = "aria-label";
            var i = 0;
            var ss = new StringBuilder();

            while (true)
            {
                var i1 = sectionPart.IndexOf(ariaText, i);
                if (i1 < 0)
                    break;

                if (sectionPart[i1 + ariaText.Length + 1] == '\'')
                {
                    var i2 = sectionPart.IndexOf('\'', i1);
                    var i3 = sectionPart.IndexOf('\'', i2 + 1);

                    var attrText = sectionPart.Substring(i2 + 1, i3 - i2 - 1);
                    attrText = MyXmlSerializer.EscapeXMLValue(attrText, true);

                    ss.Append(sectionPart.Substring(0, i2));
                    ss.Append('"');
                    ss.Append(attrText);
                    ss.Append('"');
                    var restPart = sectionPart.Substring(i3 + 1);
                    sectionPart = restPart;
                    i = 0;
                }
                else
                {
                    i += ariaText.Length;
                    continue;
                }
            }

            ss.Append(sectionPart);

            return ss.ToString();
        }

        private static string FixMankinTradImgs(string contents)
        {
            var sb = new StringBuilder();
            var mankinTradBaseLinkSlash = "http://mankin-trad.net/";
            var mankinTradBaseLinkNormal = "http://mankin-trad.net";

            int lastIndex = 0;

            while (true)
            {
                var imgIndex = contents.IndexOf("<img", lastIndex);
                if (imgIndex >= 0)
                {
                    var srcText = "src=\"";
                    var imgEndIndex = contents.IndexOf("/>", imgIndex);
                    var imgSrcIndex = contents.IndexOf(srcText, imgIndex);

                    if (imgSrcIndex >= 0 && imgEndIndex >= 0 && imgSrcIndex < imgEndIndex)
                    {
                        var sourceTextIndex = imgSrcIndex + srcText.Length;
                        var isletterI = contents[sourceTextIndex] == 'i';
                        var isLetterSlash = contents[sourceTextIndex] == '/';
                        if (isletterI || isLetterSlash)
                        {
                            // The work begins
                            sb.Append(contents.Substring(lastIndex, sourceTextIndex - lastIndex));
                            sb.Append(isLetterSlash ? mankinTradBaseLinkNormal : mankinTradBaseLinkSlash);
                            lastIndex = sourceTextIndex;
                        }
                    }
                }
                else
                    break;
            }

            sb.Append(contents.Substring(lastIndex));
            return sb.ToString();
        }

        private static string FixIncompleteImgs(string sectionPart)
        {
            var ss = new StringBuilder();

            int lastIndex = 0;
            while (true)
            {
                var i = sectionPart.IndexOf("<img", lastIndex);
                if (i < 0) break;

                var ii = sectionPart.IndexOf(">", i);

                var containsSlash = sectionPart[ii - 1] == '/';

                ss.Append(sectionPart.Substring(lastIndex, ii - lastIndex));
                if (!containsSlash)
                    ss.Append("/");
                lastIndex = ii;
            }

            if (lastIndex > 0)
            {
                ss.Append(sectionPart.Substring(lastIndex));
                return ss.ToString();
            }
            else
                return sectionPart;
        }

        private static string FixUseLinks(string s)
        {
            var useLinkFixIndex = 0;
            var sb = new StringBuilder();
            var endTag = "</use>";

            while (true)
            {
                var newIndex = s.IndexOf("<use", useLinkFixIndex);

                if (newIndex >= 0)
                {
                    sb.Append(s.Substring(useLinkFixIndex, newIndex - useLinkFixIndex));
                    useLinkFixIndex = s.IndexOf(endTag, newIndex) + endTag.Length;
                }
                else
                {
                    sb.Append(s.Substring(useLinkFixIndex, s.Length - useLinkFixIndex));
                    break;
                }
            }

            return sb.ToString();
        }

        #endregion
    }

    public class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.GZip;
            return request;
        }

        public WebResponse GetResponse(string uri)
        {
            return GetWebResponse(GetWebRequest(new Uri(uri)));
        }
    }
}
