using System.Web.Mvc;
using LocalWebServerMVC.Controllers;

namespace LocalWebServerMVC
{
    public static class Extensions
    {
        public static ActionResult Xml<Cont, T>(this Cont controller, T obj) where Cont : BaseController
        {
            var xml = new MyXmlSerializer().Serialize(obj);

            return controller.Content(xml, "application/xml");
        }
    }
}