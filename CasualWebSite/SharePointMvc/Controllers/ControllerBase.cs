using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using WebModelFactory;

namespace SharePointMvc.Controllers
{
    public class ControllerBase<T> : Controller where T : ModelFactoryBase, new()
    {
        protected T ModelFactory { get; set; }

        public HttpClient Client { get; private set; }

        protected ControllerBase()
        {
            this.ModelFactory = new T();

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            this.Client = new HttpClient(handler);
        }

        protected ActionResult Xml<E>(E obj, bool igroneXmlVersion = false)
        {
            string xml = new MyXmlSerializer().Serialize(obj, igroneXmlVersion);

            return Content(xml, "application/xml");
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
