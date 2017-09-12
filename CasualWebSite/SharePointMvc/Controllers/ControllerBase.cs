using SharePointMvc;
using System.Web.Mvc;
using WebModelFactory;

namespace CasualWebSite.Controllers
{
    public class ControllerBase<T> : Controller where T : ModelFactoryBase, new()
    {
        protected T ModelFactory { get; set; }

        protected ControllerBase()
        {
            this.ModelFactory = new T();
        }

        protected new ActionResult Content(string content, string contentType)
        {
            return base.Content(content, contentType);
        }

        protected ActionResult Xml<E>(E obj)
        {
            string xml = new MyXmlSerializer().Serialize(obj);

            return Content(xml, "application/xml");
        }
    }
}
