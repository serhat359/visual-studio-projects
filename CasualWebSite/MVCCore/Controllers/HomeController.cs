using Microsoft.AspNetCore.Mvc;
using MVCCore.Helpers;
using MVCCore.Models;
using MVCCore.Models.Home;
using System.Diagnostics;
using System.Text;

namespace MVCCore.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly CacheHelper _cacheHelper;
        private readonly MyTorrentRssHelper _myTorrentRssHelper;

        private HttpClient Client => _httpClientFactory.CreateClient();

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory, CacheHelper cacheHelper, MyTorrentRssHelper myTorrentRssHelper)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _cacheHelper = cacheHelper;
            _myTorrentRssHelper = myTorrentRssHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Json(new { message = "this page is empty" });
        }

        [Route("/test")]
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            var paths = new[]
            {
                "/Home/FixAnimeNews",
                "/Home/FixTomsArticlesManual",
                "/Home/FixTomsNewsManualParse",
                "/Home/MangainnParseXml/one-punch-man",
                "/Home/TalentlessnanaParseXml",
                "/Home/GenerateRssResult",
            };

            var baseUrl = "http://" + Request.Host.ToString();
            var client = Client;

            await Parallel.ForEachAsync(paths, async (path, ct) =>
            {
                var url = baseUrl + path;
                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("error for the url: " + url);
                }
            });

            return Json(new { message = "All tests successful" });
        }

        [HttpGet]
        public async Task<ActionResult> GetAnimeNewsCookie()
        {
            string cookieValue = _cacheHelper.Get(CacheHelper.MALCookie, () => "", CacheHelper.MALTimeSpan);
            string userAgentValue = _cacheHelper.Get(CacheHelper.MALUserAgent, () => "", CacheHelper.MALTimeSpan);

            return View(new GetAnimeNewsCookieModel { Cookie = cookieValue, UserAgent = userAgentValue });
        }

        [HttpPost]
        public async Task<ActionResult> GetAnimeNewsCookie(GetAnimeNewsCookieModel model)
        {
            _cacheHelper.Set(CacheHelper.MALCookie, model.Cookie, CacheHelper.MALTimeSpan);
            _cacheHelper.Set(CacheHelper.MALUserAgent, model.UserAgent, CacheHelper.MALTimeSpan);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> FixAnimeNews()
        {
            string url = "https://www.animenewsnetwork.com/news/rss.xml?ann-edition=us";

            string contents = (await GetUrlTextData(url, x =>
            {
                x.Headers.Add("Host", "www.animenewsnetwork.com");
                x.Headers.Add("Cookie", _cacheHelper.Get(CacheHelper.MALCookie, () => "", CacheHelper.MALTimeSpan));
                x.Headers.Add("User-Agent", _cacheHelper.Get(CacheHelper.MALUserAgent, () => "", CacheHelper.MALTimeSpan));
            }))
                .Replace("animenewsnetwork.cc", "animenewsnetwork.com")
                .Replace("http://", "https://");

            return Content(contents, "application/xml; charset=UTF-8", Encoding.UTF8);
        }

        [HttpGet]
        public async Task<IActionResult> FixTomsArticlesManual()
        {
            Func<Task<IActionResult>> initializer = async () =>
            {
                string[] urls = { "https://www.tomshardware.com/reviews",
                              "https://www.tomshardware.com/reviews/page/2",
                              "https://www.tomshardware.com/reference",
                              "https://www.tomshardware.com/features",
                              "https://www.tomshardware.com/how-to",
                              "https://www.tomshardware.com/round-up",
                              "https://www.tomshardware.com/best-picks",
                             };

                var threads = urls.Select(url => GetUrlTextData(url)).ToList();

                var elements = (await threads.AwaitAllAsync()).Select(GetRssObjectFromTomsContent).SelectMany(x => x);

                var rssObject = new RssResult(elements.Where(x => !x.Link.Contains("/news/")).OrderByDescending(x => x.PubDate));

                var xmlResult = this.Xml(rssObject);

                return xmlResult;
            };

            var xmlResultResult = await _cacheHelper.GetAsync(CacheHelper.TomsArticlesKey, initializer, TimeSpan.FromHours(2));

            return xmlResultResult;
        }

        [HttpGet]
        public async Task<IActionResult> FixTomsNewsManualParse()
        {
            string[] urls = { "https://www.tomshardware.com/news", "https://www.tomshardware.com/news/page/2" };

            var threads = urls.Select(url => GetUrlTextData(url)).ToList();

            var texts = (await threads.AwaitAllAsync()).ToList();

            var elements = texts.Select(GetRssObjectFromTomsContent).SelectMany(x => x).ToList();
            var newData = elements.OrderByDescending(c => c.PubDate).ToList();

            var cacheKey = CacheHelper.TomsNewsKey;
            var oldData = _cacheHelper.GetNotInit<List<RssResultItem>>(cacheKey);

            List<RssResultItem> data;
            if (oldData == null || oldData.Count == 0)
            {
                data = newData;
            }
            else if (newData == null || newData.Count == 0)
            {
                data = oldData;
            }
            else
            {
                var oldDate = oldData[0].PubDate;
                var newDate = newData[0].PubDate;

                if (newDate > oldDate)
                    data = newData;
                else
                    data = oldData;
            }

            _cacheHelper.Set(cacheKey, data, TimeSpan.FromHours(12));
            var rssObject = new RssResult(data);

            return this.Xml(rssObject);
        }

        [HttpGet]
        public async Task<IActionResult> MangainnParseXml(string id)
        {
            string mangaName = id;

            if (string.IsNullOrWhiteSpace(mangaName))
                throw new Exception("manganame can't be empty");

            DateTime today = DateTime.Today;

            string url = "http://www.mangainn.net/" + mangaName;

            string contents = await GetUrlTextData(url);

            string startTag = "<ul class=\"chapter-list\">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string ulPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            var document = XmlParser.Parse(ulPart);

            var rssObject = new RssResult(document.ChildNodes[0].ChildNodes.Select(liNode =>
            {
                var aNode = liNode.ChildNodes[0];
                var span1 = aNode.ChildNodes[0];
                var span2 = aNode.ChildNodes[1];

                Func<string, int> getDaysSinceRelease = s =>
                {
                    try
                    {
                        var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var num = int.Parse(parts[0]);

                        Func<string, int> parseToDays = t =>
                        {
                            switch (t)
                            {
                                case "Year":
                                case "Years":
                                    return 365;
                                case "Month":
                                case "Months":
                                    return 30;
                                case "Week":
                                case "Weeks":
                                    return 7;
                                case "Day":
                                case "Days":
                                    return 1;
                                default:
                                    return 0;
                            }
                        };

                        int days = parseToDays(parts[1]);
                        return num * days;
                    }
                    catch (Exception)
                    {
                        return 0;
                    }
                };

                var chapterName = span1.InnerText;
                var dateText = span2.InnerText;
                var daysSinceRelease = getDaysSinceRelease(dateText);
                var releaseDate = today.AddDays(-daysSinceRelease);

                return new RssResultItem
                {
                    Description = "This was parsed from mangainn.net",
                    Link = aNode.Attributes["href"],
                    PubDate = releaseDate,
                    Title = chapterName,
                };
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public async Task<IActionResult> TalentlessnanaParseXml()
        {
            DateTime today = DateTime.Today;

            var url = "https://talentlessnana.com/";

            string contents = await GetUrlTextData(url);

            string startTag = "<ul class=\"su-posts su-posts-list-loop \">";
            string endTag = "</ul>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string ulPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);

            var document = XmlParser.Parse(ulPart);

            RssResult rssObject = new RssResult(document.ChildNodes[0].ChildNodes.Select(liNode =>
            {
                var aNode = liNode.ChildNodes[0];

                var chapterName = aNode.InnerText;

                return new RssResultItem
                {
                    Description = "This was parsed from talentlessnana.com",
                    Link = aNode.Attributes["href"],
                    PubDate = today,
                    Title = chapterName,
                };
            }));

            return this.Xml(rssObject);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRssResult()
        {
            var cacheTimespan = TimeSpan.FromMinutes(15);

            Func<Task<ContentResult>> initializerFunction = async () =>
            {
                var links = _myTorrentRssHelper.GetLinks().Keys;

                var allLinks = new List<(DateTime date, XmlNode node)>();

                var tasks = links.Select(url => Task.Run(async () =>
                {
                    var key = CacheHelper.MyRssKey + ":" + url;
                    var val = await _cacheHelper.GetAsync(key, () => GetUrlTextDataWithRetry(url), cacheTimespan);
                    return val;
                })).ToArray();

                foreach (var task in tasks)
                {
                    try
                    {
                        var result = await task;
                        var xml = XmlParser.Parse(result);

                        var itemNodes = xml.ChildNodes[0].ChildNodes[0].ChildNodes.Where(x => x.TagName == "item").ToList();
                        foreach (var itemNode in itemNodes)
                        {
                            var date = DateTime.Parse(itemNode.SearchByTag("pubDate").InnerText);

                            // This line was added for browser compatibility, should not be used with bittorrent clients
                            itemNode.SearchByTag("link").InnerText = itemNode.SearchByTag("guid").InnerText;

                            allLinks.Add((date, itemNode));
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }

                var allNodes = allLinks.OrderByDescending(x => x.date).Select(x => x.node).ToList();

                var newXml = XmlParser.Parse(Resource1.RssTemplate);

                var itemNodesParent = newXml.ChildNodes[0].ChildNodes[0];
                foreach (var node in allNodes)
                {
                    itemNodesParent.AppendChild(node);
                }

                var contentResult = Content(newXml.Beautify(), "application/xml");

                return contentResult;
            };

            //return CacheHelper.Get<ContentResult>(CacheHelper.MyRssKey, initializerFunction, cacheTimespan);
            return await initializerFunction();
        }

        [HttpGet]
        public async Task<IActionResult> GetRssLinks()
        {
            var links = _myTorrentRssHelper.GetLinks();

            return View(links);
        }

        [HttpPost]
        public async Task<IActionResult> AddRssLink(string link, string name)
        {
            _myTorrentRssHelper.AddLink(link: link, name: name);
            _cacheHelper.Delete(CacheHelper.MyRssKey);

            return RedirectToAction(nameof(GetRssLinks));
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRssLink(string link)
        {
            _myTorrentRssHelper.RemoveLink(link);
            _cacheHelper.Delete(CacheHelper.MyRssKey);

            return RedirectToAction(nameof(GetRssLinks));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Private Methods
        private async Task<string> GetUrlTextData(string url, Action<HttpRequestMessage>? extraAction = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            extraAction?.Invoke(requestMessage);

            using var response = await Client.SendAsync(requestMessage);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error status code: {(int)response.StatusCode}");

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> GetUrlTextDataWithRetry(string url, Action<HttpRequestMessage>? extraAction = null)
        {
            Func<HttpRequestMessage> requestMessageCreator = () =>
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
                extraAction?.Invoke(requestMessage);
                return requestMessage;
            };

            using var response = await Client.SendWithRetryAsync(requestMessageCreator);
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error status code: {(int)response.StatusCode}");

            return await response.Content.ReadAsStringAsync();
        }

        private IEnumerable<RssResultItem> GetRssObjectFromTomsContent(string contents)
        {
            string startTag = "<section data-next=\"latest\"";
            string endTag = "</section>";

            int indexOfStart = contents.IndexOf(startTag);
            int indexOfEnd = contents.IndexOf(endTag, indexOfStart);

            string sectionPart = contents.Substring(indexOfStart, indexOfEnd - indexOfStart + endTag.Length);
            sectionPart = FixIncompleteImgs(sectionPart); // This part is necessary unfortunately

            var document = XmlParser.Parse(sectionPart);

            var divs = document.ChildNodes[0].GetAllNodesRecursive().Where(c =>
            {
                var classValue = c.Attributes["class"];
                var dataPageValue = c.Attributes["data-page"];
                return classValue?.Contains("listingResult small") == true
                    && dataPageValue != null
                    && classValue?.Contains("sponsored") != true;
            }).ToList();

            var elements = divs.Select(liNode =>
            {
                var aNode = liNode.SearchByTag("a");
                var link = aNode.Attributes["href"];

                var imgNode = liNode.SearchByTag("img");
                var imgSrc = imgNode.Attributes["data-srcset"]?.BeforeFirst(" ") ?? imgNode.Attributes["data-src"] ?? imgNode.Attributes["src"];

                var img = $"<a href=\"{link}\"><img src=\"{imgSrc}\" /></a>";

                return new RssResultItem
                {
                    Description = $"<![CDATA[{img}]]>",
                    Link = link,
                    PubDate = DateTime.Parse(liNode.SearchByTag("time").Attributes["datetime"]),
                    Title = liNode.SearchByTag("h3").InnerText,
                };
            });

            return elements;
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

                ss.Append(sectionPart.AsSpan(lastIndex, ii - lastIndex));
                if (!containsSlash)
                    ss.Append('/');
                lastIndex = ii;
            }

            if (lastIndex > 0)
            {
                ss.Append(sectionPart.AsSpan(lastIndex));
                return ss.ToString();
            }
            else
                return sectionPart;
        }
        #endregion
    }
}