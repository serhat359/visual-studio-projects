using System.Collections.Generic;

namespace DotNetCoreWebsite.Models
{
    public class YtsResponseModel
    {
        public string status { get; set; } = "";

        public string message { get; set; } = "";

        public List<YtsResponseDataModel> data { get; set; } = new();
    }

    public class YtsResponseDataModel
    {
        public string url { get; set; } = "";

        public string img { get; set; } = "";

        public string title { get; set; } = "";

        public string year { get; set; } = "";

        public string urlStripped => url.StartsWith("https://yts.mx") ? url.Replace("https://yts.mx", "") : "";
    }
}
