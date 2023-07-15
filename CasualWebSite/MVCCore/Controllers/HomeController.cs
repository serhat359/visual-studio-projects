using Microsoft.AspNetCore.Mvc;
using MVCCore.Helpers;
using MVCCore.Models;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace MVCCore.Controllers
{
    public partial class HomeController : BaseController
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
                "/Home/FixTomsArticlesManual",
                "/Home/FixTomsNewsManualParse",
                "/Home/MangainnParseXml/one-punch-man",
                "/Home/TalentlessnanaParseXml",
                "/Home/GenerateRssResult",
                "/Home/CurrencyExchangeRatio",
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
        public async Task<IActionResult> CurrencyExchangeRatio()
        {
            var client = Client;

            async Task<string> ExtractRatio(string url)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/110.0");
                    using var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    var data = await response.Content.ReadAsStringAsync();
                    var classIndex = data.IndexOf(" fxKbKc");
                    if (classIndex < 0)
                        throw new Exception("classIndex");
                    var beginIndex = data.IndexOf(">", classIndex) + 1;
                    if (beginIndex < 0)
                        throw new Exception("beginIndex");
                    var endIndex = data.IndexOf('<', beginIndex);
                    if (endIndex < 0)
                        throw new Exception("endIndex");
                    var text = data[beginIndex..endIndex];
                    return text;
                }
                catch (Exception e)
                {
                    throw new Exception($"Error with the url: {url}", e);
                }
            }

            // All urls are EUR based
            var urls = new List<(string key, string url)>()
            {
                { ("USD", "https://www.google.com/finance/quote/EUR-USD") },
                { ("JPY", "https://www.google.com/finance/quote/EUR-JPY") },
                { ("TRY", "https://www.google.com/finance/quote/EUR-TRY") },
            };

            var urlTasks = urls.Select(x => (x.key, data: ExtractRatio(url: x.url))).ToList();
            var dataRatioEurUsd = decimal.Parse(await urlTasks[0].data);

            var resultData = new Dictionary<string, CurrencyElement>();
            resultData["EUR"] = new CurrencyElement
            {
                code = "EUR",
                value = Math.Round(1m / dataRatioEurUsd, 6),
            };
            foreach (var (key, task) in urlTasks)
            {
                resultData[key] = new CurrencyElement
                {
                    code = key,
                    value = Math.Round(decimal.Parse(await task) / dataRatioEurUsd, 6),
                };
            }

            return Json(new
            {
                meta = new { },
                data = resultData,
            });
        }

        [HttpGet]
        public async Task<IActionResult> Bun()
        {
            static int ParseMonthName(string s)
            {
                switch (s.ToLowerInvariant())
                {
                    case "january": return 1;
                    case "february": return 2;
                    case "march": return 3;
                    case "april": return 4;
                    case "may": return 5;
                    case "june": return 6;
                    case "july": return 7;
                    case "august": return 8;
                    case "september": return 9;
                    case "october": return 10;
                    case "november": return 11;
                    case "december": return 12;
                    default:
                        throw new Exception();
                }
            }

            string url = "https://bun.sh/blog";
            string baseUrl = "https://bun.sh";

            var allContent = await GetUrlTextData(url);

            var start = allContent.IndexOf("<section");
            if (start < 0)
                throw new Exception();

            var endPart = "</section>";
            var end = allContent.IndexOf(endPart, start);
            if (end < 0)
                throw new Exception();

            var content = allContent[start..(end + endPart.Length)];

            var parsed = XmlParser.ParseHtml(content);

            var allNodes = parsed.ChildNodes[0].GetAllNodesRecursive();
            var hrefs = allNodes.Where(x => x.TagName == "a" && (x.Attributes["class"] ?? "").StartsWith("no-underline")).ToList();

            var pubDateRegex = BunPubDateRegex();
            var data = hrefs.Select(x =>
            {
                var pubDateStr = x.GetAllNodesRecursive().Where(x => x.TagName == "span" && (x.Attributes["class"] ?? "").StartsWith("text-gray-600")).Skip(1).First().InnerText;
                var match = pubDateRegex.Match(pubDateStr);
                if (!match.Success)
                    throw new Exception();

                var month = match.Groups[1].Value;
                var day = match.Groups[2].Value;
                var year = match.Groups[3].Value;

                return new RssResultItem
                {
                    Title = x.GetAllNodesRecursive().First(x => x.TagName == "div" && (x.Attributes["class"] ?? "").StartsWith("mb-1")).InnerText,
                    Link = baseUrl + x.Attributes["href"],
                    Description = x.GetAllNodesRecursive().First(x => x.TagName == "p" && (x.Attributes["class"] ?? "").StartsWith("text-lg")).InnerText,
                    PubDate = new DateTime(year: int.Parse(year), month: ParseMonthName(month), day: int.Parse(day)),
                };
            }).ToList();

            var rssObject = new RssResult(data);
            return Xml(rssObject);
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

                var xmlResult = Xml(rssObject);

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

            return Xml(rssObject);
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

            return Xml(rssObject);
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

            return Xml(rssObject);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRssResult()
        {
            return await generateRssResultInner(useRealLinks: false);
        }

        [HttpGet]
        public async Task<IActionResult> GenerateRealRssResult()
        {
            return await generateRssResultInner(useRealLinks: true);
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
            sectionPart = sectionPart.Replace("</source>", ""); // This part is necessary unfortunately, Tom's html is broken

            var document = XmlParser.Parse(sectionPart, isHtml: true);

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

        private async Task<IActionResult> generateRssResultInner(bool useRealLinks)
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
                            if (!useRealLinks)
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
                    itemNodesParent.AddXmlNode(node);
                }

                var contentResult = Content(newXml.Beautify(), "application/xml");

                return contentResult;
            };

            //return CacheHelper.Get<ContentResult>(CacheHelper.MyRssKey, initializerFunction, cacheTimespan);
            return await initializerFunction();
        }

        [GeneratedRegex("([a-zA-Z]+) ([0-9]+), ([0-9]+)", RegexOptions.Compiled)]
        private static partial Regex BunPubDateRegex();
        #endregion
    }

    class CurrencyElement
    {
        public string? code { get; set; }
        public decimal value { get; set; }
    }
}