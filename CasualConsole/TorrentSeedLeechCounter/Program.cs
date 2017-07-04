using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;

namespace TorrentSeedLeechCounter
{
    public class Program
    {
        const int intervalSeconds = 60;
        const int oneMinuteMillis = 1000 * intervalSeconds;
        const string searchQuery = "horriblesubs 720p ashibe 45";
        const string urlBase = "https://nyaa.si/?page=rss&q={0}&c=1_2&f=0";
        const string trackerUrl = "http://nyaa.tracker.wf:7777/announce";

        public static void Main(string[] args)
        {
            bool isDatabaseAvailable = Database.CheckAvailability();

            if (!isDatabaseAvailable)
                throw new Exception("Cannot connect to database, please check if the database server is running.");

            string url = string.Format(urlBase, Uri.EscapeDataString(searchQuery));

            string torrentHash = GetTorrentHashFromUrl(url);

            while (true)
            {
                string torrentAnnounceInfo = GetTorrentInfoFromTracker(torrentHash);

                TorrentAnnounceInfo parsedTorrentAnnounceInfo = ParseTorrentInfo(torrentAnnounceInfo, torrentHash);

                InsertInfoToDatabase(parsedTorrentAnnounceInfo);

                WriteToConsole("Added info to database, will check again {0} seconds.", intervalSeconds);
                Thread.Sleep(oneMinuteMillis);
            }
        }

        private static string GetTorrentHashFromUrl(string url)
        {
            string torrentHash = GetTorrentHash(GetRSSText(url));

            while (torrentHash == null)
            {
                WriteToConsole("The search did not yield any results, trying in {0} seconds...", intervalSeconds);
                Thread.Sleep(oneMinuteMillis);
                torrentHash = GetTorrentHash(GetRSSText(url));
            }

            return torrentHash;
        }

        private static TorrentAnnounceInfo ParseTorrentInfo(string torrentAnnounceInfo, string infoHash)
        {
            string[] parts = torrentAnnounceInfo.Split(':');

            int interval = int.Parse(parts.First(x => x.StartsWith("intervali")).Replace("intervali", "").Split('e')[0]);
            int minInterval = int.Parse(parts.First(x => x.StartsWith("min intervali")).Replace("min intervali", "").Split('e')[0]);
            int peers = int.Parse(parts.First(x => x.StartsWith("peers")).Replace("peers", "").Split('e')[0]);
            int complete = int.Parse(parts.First(x => x.StartsWith("completei")).Replace("completei", "").Split('e')[0]);
            int incomplete = int.Parse(parts.First(x => x.StartsWith("incompletei")).Replace("incompletei", "").Split('e')[0]);

            return new TorrentAnnounceInfo
            {
                FullText = torrentAnnounceInfo,
                Complete = complete,
                Incomplete = incomplete,
                Interval = interval,
                MinInterval = minInterval,
                Peers = peers,
                TorrentHash = infoHash
            };
        }

        private static string GetTorrentInfoFromTracker(string torrentHash)
        {
            string byteHash = ConvertHashToByteHash(torrentHash);

            Dictionary<string, string> urlParams = new Dictionary<string, string>();
            urlParams.Add("info_hash", byteHash);
            urlParams.Add("peer_id", "ABCDABCDABCDABCDABCD");
            urlParams.Add("left", "0");
            urlParams.Add("uploaded", "0");
            urlParams.Add("downloaded", "0");
            urlParams.Add("port", "6882");
            urlParams.Add("compact", "1");

            var combinedUrl = trackerUrl + "?" + string.Join("&", urlParams.Select(x => x.Key + "=" + x.Value));

            string data = GetUrlTextData(combinedUrl);

            return data;
        }

        private static string ConvertHashToByteHash(string torrentHash)
        {
            StringBuilder builder = new StringBuilder(torrentHash.Length / 2 * 3);
            for (int i = 0; i < torrentHash.Length / 2; i++)
            {
                builder.Append('%');
                builder.Append(torrentHash[i * 2]);
                builder.Append(torrentHash[i * 2 + 1]);
            }

            string byteHash = builder.ToString();
            return byteHash;
        }

        private static void InsertInfoToDatabase(TorrentAnnounceInfo parsedTorrentAnnounceInfo)
        {
            Database.Insert(parsedTorrentAnnounceInfo);
        }

        private static string GetTorrentHash(string rssText)
        {
            XmlDocument document = new XmlDocument();

            document.LoadXml(rssText);

            XmlNodeList itemList = document.FirstChild.FirstChild.SelectNodes("item");

            if (itemList.Count == 0)
                return null;
            else if (itemList.Count > 1)
                throw new Exception("I was expecting only one item for this rss");

            XmlNode item = itemList[0];

            XmlNode infoHashNode = item.ChildNodes.AsIterable<XmlNode>().First(x => x.Name.Equals("nyaa:infoHash"));

            string infoHash = infoHashNode.InnerText;

            return infoHash;
        }

        private static string GetRSSText(string url)
        {
            string rssText = GetUrlTextData(url);

            while (rssText == null)
            {
                WriteToConsole("Could not get rss data, trying in {0} seconds...", intervalSeconds);
                Thread.Sleep(oneMinuteMillis);
                rssText = GetUrlTextData(url);
            }

            return rssText;
        }

        private static string GetUrlTextData(string url)
        {
            string s;

            using (WebClient client = new WebClient())
            {
                while (true)
                {
                    try
                    {
                        s = client.DownloadString(url);
                        break;
                    }
                    catch
                    {
                        WriteToConsole("Could not get the data from url: {0}" + "\n" + "Will try again in {1} seconds...", url, intervalSeconds);
                        Thread.Sleep(oneMinuteMillis);
                    }
                }
            }

            return s;
        }

        private static void WriteToConsole(string baseText, params object[] values)
        {
            DateTime now = DateTime.Now;

            Console.Write(baseText, values);
            Console.WriteLine("\t{0}", now);
            Console.WriteLine();
        }
    }

    class TorrentAnnounceInfo
    {
        public string FullText { get; set; }
        public int? Interval { get; set; }
        public int? MinInterval { get; set; }
        public int? Peers { get; set; }
        public int? Complete { get; set; }
        public int? Incomplete { get; set; }
        public string TorrentHash { get; set; }
    }
}
