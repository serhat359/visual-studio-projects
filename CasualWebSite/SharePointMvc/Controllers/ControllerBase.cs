using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Mvc;

namespace SharePointMvc.Controllers
{
    public class ControllerBase : Controller
    {
        public HttpClient Client { get; private set; }

        protected ControllerBase()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            this.Client = new HttpClient(new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            });
        }

        protected ActionResult Xml<E>(E obj, bool igroneXmlVersion = false)
        {
            string xml = new MyXmlSerializer().Serialize(obj, igroneXmlVersion);

            ContentResult content = Content(xml, "application/xml");
            content.ContentEncoding = Encoding.UTF8;
            return content;
        }

        protected ActionResult ExcelFile(byte[] byteArray, string fileNameWithExtension)
        {
            return File(byteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileNameWithExtension);
        }

        protected ActionResult TorrentFile(byte[] byteArray, string fileNameWithExtension)
        {
            return File(byteArray, "application/x-bittorrent", fileNameWithExtension);
        }
    }
}
