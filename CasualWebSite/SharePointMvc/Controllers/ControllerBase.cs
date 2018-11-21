﻿using System.Web.Mvc;
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

        protected ActionResult Xml<E>(E obj, bool xmlEncode = true)
        {
            string xml = new MyXmlSerializer().Serialize(obj, xmlEncode);

            return Content(xml, "application/xml");
        }
    }
}
