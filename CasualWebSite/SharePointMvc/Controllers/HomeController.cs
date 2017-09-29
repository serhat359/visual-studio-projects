using System.Web.Mvc;
using WebModelFactory;
using Model.Web;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using System.Net;
using System.Text;
using System.Linq;
using Newtonsoft.Json;

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
        public ActionResult Pokemon()
        {
            PokemonModel model = base.ModelFactory.LoadCasual();

            model.Query = "order by greatest(attack,spattack)*speed desc";

            return View(model);
        }

        [HttpGet]
        public ActionResult Download(string path)
        {
            var rangeResult = Request.Params["HTTP_RANGE"];

            string root = @"C:\Users\Xhertas\";

            string fullPath = root + path;

            string extension = Path.GetExtension(fullPath);

            if (rangeResult == null)
            {
                string requestStr = ModelFactoryBase.Stringify(Request);

                var fileStream = new FileStream(fullPath, FileMode.Open);

                return File(fileStream, "application/unknown", "new file" + extension);
            }
            else
            {
                long bytesToSkip = long.Parse(Regex.Match(rangeResult, @"\d+").Value, NumberFormatInfo.InvariantInfo);
                
                long fSize = (new System.IO.FileInfo(fullPath)).Length;
                
                var fileStream = new FileStream(fullPath, FileMode.Open);
                fileStream.Position = bytesToSkip;

                var result = File(fileStream, "application/unknown", "new file" + extension);

                long startbyte = 0;
                long endbyte = fSize - 1;
                long desSize = endbyte - startbyte + 1;
                Response.StatusCode = 206;
                Response.AddHeader("Content-Length", desSize.ToString());
                Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", startbyte, endbyte, fSize));
                //Data

                return result;
            }
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

            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                s = client.DownloadString(url);
            }

            return s;
        }

        #endregion
    }
}
