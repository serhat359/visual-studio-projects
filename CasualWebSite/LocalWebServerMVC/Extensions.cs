using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using LocalWebServerMVC.Controllers;

namespace LocalWebServerMVC
{
    public static class Extensions
    {
        public static ActionResult Xml<Cont, T>(this Cont controller, T obj) where Cont : BaseController
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            var xml = "";

            using (var sww = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, obj);
                    xml = sww.ToString(); // Your XML
                }
            }

            return controller.Content(xml, "application/xml");
        }
    }
}