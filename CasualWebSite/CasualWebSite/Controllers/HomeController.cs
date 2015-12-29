using System.Web.Mvc;
using WebModelFactory;
using Model.Web;

namespace CasualWebSite.Controllers
{
    [HandleError]
    public class HomeController : ControllerBase<HomeModelFactory>
    {
        #region Get Methods
        public ActionResult Index()
        {
            ViewData["Message"] = "Welcome to ASP.NET MVC!";

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Pokemon()
        {
            PokemonModel model = base.ModelFactory.LoadCasual();

            return View(model);
        }
        #endregion

        #region Post Methods
        [HttpPost]
        public ActionResult Pokemon(PokemonModel request)
        {
            PokemonModel model = ModelState.IsValid ? base.ModelFactory.LoadCasual(request) : base.ModelFactory.LoadCasual();

            return View(model);
        }
        #endregion
    }
}
