using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MVCCore;

public class MyXmlResult : IActionResult
{
    private readonly object obj;

    public MyXmlResult(object obj)
    {
        this.obj = obj;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var response = context.HttpContext.Response;
        response.ContentType = "application/xml";
        await new MyXmlSerializer().SerializeToStreamAsync(obj, response.Body, igroneXmlVersion: false);
        await response.CompleteAsync();
    }
}
