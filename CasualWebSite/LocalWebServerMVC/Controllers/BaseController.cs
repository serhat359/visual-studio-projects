using System.Web.Mvc;

namespace LocalWebServerMVC.Controllers
{
    public class BaseController : Controller
    {
        public new ActionResult Content(string content, string contentType)
        {
            return base.Content(content, contentType);
        }
    }
}