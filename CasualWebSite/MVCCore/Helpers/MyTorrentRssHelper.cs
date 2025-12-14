using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace MVCCore.Helpers;

public class MyTorrentRssHelper
{
    private Dictionary<string, string>? links;
    private readonly string repositoryFileName = "rssLinks.json";

    private string FileName { get { return repositoryFileName; } }

    public MyTorrentRssHelper()
    {
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        File.WriteAllText(path: FileName, contents: JsonSerializer.Serialize(this.links, options));
    }

    private void CheckLinks()
    {
        if (this.links == null)
        {
            string jsonText = File.ReadAllText(FileName);
            try
            {
                this.links = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonText);
            }
            catch (Exception)
            {
                this.links = new Dictionary<string, string>();
            }
        }
    }
}
