using Microsoft.AspNetCore.Mvc;

namespace MVCCore.Controllers;

public static class ControllerExtensions
{
    public static MyXmlResult Xml<E>(this Controller controller, E obj) where E : notnull
    {
        return new MyXmlResult(obj);
    }
}