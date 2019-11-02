using System.Web.Mvc;
using WebModelFactory;

namespace SharePointMvc.Controllers
{
    public class ControllerBase<T> : Controller where T : ModelFactoryBase, new()
    {
        protected T ModelFactory { get; set; }

        protected ControllerBase()
        {
            this.ModelFactory = new T();
        }

        protected ActionResult Xml<E>(E obj, bool igroneXmlVersion = false)
        {
            string xml = new MyXmlSerializer().Serialize(obj, igroneXmlVersion);

            return Content(xml, "application/xml");
        }

        protected  ActionResult ExcelFile(byte[] byteArray, string fileNameWithExtension)
        {
            return File(byteArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileNameWithExtension);
        }

        protected ActionResult TorrentFile(byte[] byteArray, string fileNameWithExtension)
        {
            return File(byteArray, "application/x-bittorrent", fileNameWithExtension);
        }
    }
}
