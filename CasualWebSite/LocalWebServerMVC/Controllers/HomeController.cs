using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using LocalWebServerMVC.Models;
using Newtonsoft.Json;

namespace LocalWebServerMVC.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return Json(new { message = "this page is empty" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult YoutubeMerryChristmas()
        {
            string jsonResult = GetJsonSearchResult();

            string youtubeWatchBaseUrl = "https://www.youtube.com/watch?v={0}";

            YoutubeSearchResult youtubeSearchResult = JsonConvert.DeserializeObject<YoutubeSearchResult>(jsonResult);

            RssResult rssObject = new RssResult(youtubeSearchResult.items.Select(x => new RssResultItem
            {
                description = x.snippet.description,
                link = string.Format(youtubeWatchBaseUrl, x.id.videoId),
                pubDate = x.snippet.publishedAt,
                title = x.snippet.title,
            }));

            return this.Xml(rssObject);
        }

        private static string GetJsonSearchResult()
        {
            string q = "オーディオドラマ";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("order", "date");
            parameters.Add("part", "snippet");
            parameters.Add("channelId", "UCpRh2xmGtaVhFVuyCB271pw");
            parameters.Add("key", "AIzaSyBrbMk0ZR5638IrTlVBTra7NLYdoBjUt2o");
            parameters.Add("maxResults", "50");
            parameters.Add("q", Uri.EscapeUriString(q));

            string baseurl = "https://www.googleapis.com/youtube/v3/search";

            string wholeUrl = string.Format("{0}?{1}", baseurl, string.Join("&", parameters.Select(pair => pair.Key + "=" + pair.Value)));

            string jsonResult = GetUrlTextData(wholeUrl);
            return jsonResult;
        }

        private static string GetUrlTextData(string url)
        {
            string s;

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                s = client.DownloadString(url);
            }

            return s;
        }

    }
}
