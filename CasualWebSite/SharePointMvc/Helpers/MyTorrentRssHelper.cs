using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SharePointMvc.Helpers
{
    public class MyTorrentRssHelper
    {
        private static MyTorrentRssHelper instance;
        private Dictionary<string, string> links;
        private readonly string repositoryFileName = "rssLinks.json";
        private string basePath;
        private string FileName { get { return basePath + repositoryFileName; } }

        public static MyTorrentRssHelper Instance(string basePath)
        {
            return instance ?? (instance = new MyTorrentRssHelper(basePath));
        }

        private MyTorrentRssHelper(string basePath)
        {
            this.basePath = basePath;

            if (!File.Exists(FileName))
            {
                links = new Dictionary<string, string>();
                SaveLinks();
            }
        }

        public Dictionary<string, string> GetLinks()
        {
            CheckLinks();

            return links;
        }

        public void AddLink(string link, string name)
        {
            links.Add(key: link, value: name);

            SaveLinks();
        }

        public bool RemoveLink(string link)
        {
            bool removeResult = links.Remove(link);

            SaveLinks();

            return removeResult;
        }

        private void SaveLinks()
        {
            File.WriteAllText(path: FileName, contents: JsonConvert.SerializeObject(this.links));
        }

        private void CheckLinks()
        {
            if (this.links == null)
            {
                string jsonText = File.ReadAllText(FileName);
                try
                {
                    this.links = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonText);
                }
                catch (Exception)
                {
                    this.links = new Dictionary<string, string>();
                }
            }
        }
    }
}