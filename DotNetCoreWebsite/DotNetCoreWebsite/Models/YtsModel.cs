using System.Collections.Generic;

namespace DotNetCoreWebsite.Models
{
    public class YtsModel
    {
        public string Query { get; set; } = "";

        public List<YtsResponseDataModel> ResponseData { get; set; } = new();
    }
}
