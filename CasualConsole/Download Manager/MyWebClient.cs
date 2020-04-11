using System;
using System.Net;

namespace Download_Manager
{
    public class MyWebClient : WebClient
    {
        private readonly long from;

        public MyWebClient(long from)
        {
            this.from = from;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.AddRange(this.from);
            return request;
        }
    }
}
