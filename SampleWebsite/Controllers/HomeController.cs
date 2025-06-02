using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SampleWebsite.Models;
using SampleWebsite.Models.Home;

namespace SampleWebsite.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Privacy()
    {
        var model = new PrivacyModel();
        return View(model);
    }

    [HttpPost]
    public IActionResult Privacy(PrivacyModel model)
    {
        if (model.Month != null && !model.MonthList.Select(x => x.Value).Contains($"{model.Month}"))
        {
            ModelState.AddModelError(nameof(model.Month), "Please choose an item from the list");
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
