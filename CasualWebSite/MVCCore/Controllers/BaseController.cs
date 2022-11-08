using Microsoft.AspNetCore.Mvc;

namespace MVCCore.Controllers
{
    public class BaseController : Controller
    {
        protected ActionResult Xml<E>(E obj, bool igroneXmlVersion = false)
        {
            string xml = new MyXmlSerializer().Serialize(obj, igroneXmlVersion);

            ContentResult content = Content(xml, "application/xml");
            return content;
        }
    }
}
