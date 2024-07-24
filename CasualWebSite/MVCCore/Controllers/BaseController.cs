using Microsoft.AspNetCore.Mvc;

namespace MVCCore.Controllers;

public class BaseController : Controller
{
    protected MyXmlResult Xml<E>(E obj)
    {
        return new MyXmlResult(obj);
    }
}
