using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
    }
}
